using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using RentalManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Security.Claims;

namespace RentalManagementSystem.Controllers
{
	[Route("Tenant/Payments")]
	public class TPaymentController : Controller
	{
		private readonly RentalManagementContext _context;

		public TPaymentController(RentalManagementContext context)
		{
			_context = context;
		}

		[HttpGet]
		[Route("")]
		public async Task<IActionResult> Payment(int page = 1, int pageSize = 10)
		{
			// Extract user ID and role
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var userRole = User.FindFirst(ClaimTypes.Role)?.Value; // Use standard ClaimTypes.Role

			// Validate claims
			if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(userRole))
			{
				return Unauthorized("User ID or role claim is missing.");
			}

			if (!int.TryParse(userIdStr, out var userId))
			{
				return BadRequest("Invalid user ID format.");
			}

			// Fetch user and associated house
			var user = await _context.Users
				.Include(u => u.House)
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
			{
				return NotFound("User not found.");
			}

			// Prepare payments query
			var query = _context.Payments.AsQueryable();

			if (userRole == "Tenant")
			{
				query = query.Where(p => p.UserId == userId);
			}

			// Fetch payments with pagination
			var totalPayments = await query.CountAsync();
			var payments = await query
				.Include(p => p.House)
				.Include(p => p.User)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(page => new PaymentListItemViewModel
				{
					Id = page.Id,
					PaymentType = page.PaymentType.ToString(),
					Amount = page.Amount,
					PaymentDate = page.PaymentDate,
					PaymentMethod = page.PaymentMethod.ToString(),
					PaymentStatus = page.PaymentStatus.ToString(),
					PaymentReference = page.PaymentReference,
					HouseId = page.HouseId,
					UserId = page.UserId,
					House = page.House,
					User = page.User
				})
				.ToListAsync();

			// Fetch houses for dropdown
			var houses = await _context.Houses
				.Select(h => new SelectListItem
				{
					Value = h.Id.ToString(),
					Text = h.HouseNumber
				})
				.ToListAsync();

			// Prepare ViewModel
			var viewModel = new PaymentViewModel
			{
				Payments = payments,
				Houses = houses,
				HouseId = user.HouseId,
				UserId = userId
			};

			return View("~/Views/Tenant/Payments.cshtml", viewModel);
		}


		// Add or update these methods in your TPaymentController class

		[HttpGet]
		[Route("GetHouseRent/{houseId}")]
		public async Task<IActionResult> GetHouseRent(int houseId)
		{
			var house = await _context.Houses.FindAsync(houseId);
			if (house == null)
				return NotFound();

			return Json(new { rent = house.Rent });
		}

		[HttpGet]
		[Route("GetUnpaidUtilities")]
		public async Task<IActionResult> GetUnpaidUtilities()
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
			{
				return BadRequest("Invalid user ID");
			}

			var unpaidUtilities = await _context.UtilityReadings
				.Include(u => u.Utility)
				.Where(u => u.TenantId == userId && !u.IsPaid)
				.Select(u => new
				{
					id = u.Id,
					name = u.Utility.Name,
					totalCost = u.TotalCost
				})
				.ToListAsync();

			return Json(unpaidUtilities);
		}


		[HttpPost]
[Route("Add")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Add(string PaymentType, decimal Amount, DateTime PaymentDate, string PaymentMethod, string PaymentStatus, string Description, int? HouseId, int? UserId)
{
    try
    {
        // Get user ID from claims
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            TempData["ErrorMessage"] = "Invalid or missing user ID.";
            return RedirectToAction(nameof(Payment));
        }

        // Debuging information
        Console.WriteLine($"PaymentType: {PaymentType}");
        Console.WriteLine($"Amount: {Amount}");
        Console.WriteLine($"PaymentDate: {PaymentDate}");
        Console.WriteLine($"PaymentMethod: {PaymentMethod}");
        Console.WriteLine($"PaymentStatus: {PaymentStatus}");
        
        // Validate inputs manually
        if (string.IsNullOrEmpty(PaymentType) || Amount <= 0 || string.IsNullOrEmpty(PaymentMethod) || string.IsNullOrEmpty(PaymentStatus))
        {
            TempData["ErrorMessage"] = "Please fill in all required fields with valid values.";
            return RedirectToAction(nameof(Payment));
        }

        // Check if payment already exists for current month
        var currentDate = DateTime.Now;
        var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var existingPayment = await _context.Payments
            .Where(p => p.UserId == userId
                    && p.PaymentType == PaymentType
                    && p.PaymentDate >= firstDayOfMonth
                    && p.PaymentDate <= lastDayOfMonth)
            .FirstOrDefaultAsync();

        if (existingPayment != null)
        {
            var monthYear = currentDate.ToString("MMMM yyyy");
            TempData["ErrorMessage"] = $"{PaymentType} payment for {monthYear} has already been made. Next payment can be made in {currentDate.AddMonths(1).ToString("MMMM yyyy")}.";
            return RedirectToAction(nameof(Payment));
        }

        // Get user and associated house
        var user = await _context.Users
            .Include(u => u.House)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found in the database.";
            return RedirectToAction(nameof(Payment));
        }

        // Use house from user
        HouseId = user.HouseId;

        decimal totalAmount = Amount;
        List<UtilityReading> unpaidUtilities = new List<UtilityReading>();

        // Begin transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (PaymentType == "Utility")
            {
                unpaidUtilities = await _context.UtilityReadings
                    .Include(u => u.Utility)
                    .Where(u => u.TenantId == userId && !u.IsPaid)
                    .ToListAsync();

                if (!unpaidUtilities.Any())
                {
                    TempData["ErrorMessage"] = "No unpaid utilities found for the current month.";
                    return RedirectToAction(nameof(Payment));
                }

                totalAmount = unpaidUtilities.Sum(u => u.TotalCost);
            }
            else if (PaymentType == "Rent")
            {
                if (user.House == null)
                {
                    TempData["ErrorMessage"] = "No house associated with user.";
                    return RedirectToAction(nameof(Payment));
                }
                totalAmount = user.House.Rent;
            }

            // Create payment record
            var payment = new Payment
            {
                Amount = totalAmount,
                PaymentDate = PaymentDate,
                PaymentMethod = PaymentMethod,
                PaymentStatus = PaymentStatus,
                PaymentReference = GeneratePaymentReference(HouseId, userId),
                Description = string.IsNullOrEmpty(Description) ? 
                    GeneratePaymentDescription(PaymentType, unpaidUtilities) : 
                    Description,
                HouseId = HouseId,
                UserId = userId,
                PaymentType = PaymentType
            };

            _context.Payments.Add(payment);

            // Mark utilities as paid if applicable
            if (PaymentType == "Utility")
            {
                foreach (var utility in unpaidUtilities)
                {
                    utility.IsPaid = true;
                    _context.Update(utility);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = $"{PaymentType} payment of ${totalAmount:F2} processed successfully.";
            return RedirectToAction(nameof(Payment));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = $"Failed to process payment: {ex.Message}";
            return RedirectToAction(nameof(Payment));
        }
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
        return RedirectToAction(nameof(Payment));
    }
}

		private string GeneratePaymentDescription(string paymentType, List<UtilityReading> utilities)
		{
			var monthYear = DateTime.Now.ToString("MMMM yyyy");

			if (paymentType == "Rent")
			{
				return $"Rent payment for {monthYear}";
			}
			else if (paymentType == "Utility" && utilities.Any())
			{
				var utilityDetails = utilities
					.Select(u => $"{u.Utility.Name}: ${u.TotalCost}")
					.ToList();
				return $"Utility payment for {monthYear} - {string.Join(", ", utilityDetails)}";
			}

			return $"{paymentType} payment for {monthYear}";
		}

		[HttpGet]
		[Route("CheckPaymentAvailability")]
		public async Task<IActionResult> CheckPaymentAvailability(string paymentType)
		{
			var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
			{
				return Json(new { available = false, message = "Invalid user ID" });
			}

			var currentDate = DateTime.Now;
			var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

			var existingPayment = await _context.Payments
				.Where(p => p.UserId == userId
						&& p.PaymentType == paymentType
						&& p.PaymentDate >= firstDayOfMonth
						&& p.PaymentDate <= lastDayOfMonth)
				.FirstOrDefaultAsync();

			if (existingPayment != null)
			{
				var nextPaymentMonth = currentDate.AddMonths(1).ToString("MMMM yyyy");
				return Json(new
				{
					available = false,
					message = $"{paymentType} payment for this month has already been made. Next payment can be made in {nextPaymentMonth}."
				});
			}

			return Json(new { available = true });
		}

		[HttpPost]
		[Route("Edit/{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Amount,PaymentDate,PaymentMethod,PaymentStatus,PaymentReference,Description,HouseId,UserId")] PaymentDto paymentDto)
		{
			var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

			var payment = await _context.Payments.FindAsync(id);
			if (payment == null)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				payment.Amount = paymentDto.Amount;
				payment.PaymentDate = paymentDto.PaymentDate;
				payment.PaymentMethod = paymentDto.PaymentMethod.ToString();
				payment.PaymentStatus = paymentDto.PaymentStatus.ToString();
				payment.PaymentReference = GeneratePaymentReference(paymentDto.HouseId, paymentDto.UserId);
				payment.Description = paymentDto.Description;

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Payment));
			}

			return RedirectToAction(nameof(Payment));
		}

		[Route("Delete/{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

			var payment = await _context.Payments.FindAsync(id);
			if (payment == null)
			{
				return NotFound();
			}

			_context.Payments.Remove(payment);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Payment));
		}

		private bool PaymentExists(int id)
		{
			return _context.Payments.Any(e => e.Id == id);
		}

		private string GeneratePaymentReference(int? houseId, int? userId)
		{
			if (houseId.HasValue && userId.HasValue)
			{
				var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
				return $"{houseId}-{userId}-{timestamp}";
			}

			return "REF-UNKNOWN";
		}

		[HttpGet]
		[Route("GetPaymentDetails")]
		public async Task<IActionResult> GetPaymentDetails(int paymentType, int houseId)
		{
			try
			{
				var house = await _context.Houses
					.Include(h => h.Property)
					.ThenInclude(p => p.User)
					.ThenInclude(u => u.Profile)
					.FirstOrDefaultAsync(h => h.Id == houseId);

				if (house == null)
				{
					return NotFound("House not found.");
				}

				var landlordProfile = house.Property.User.Profile;

				if (landlordProfile == null)
				{
					return NotFound("Landlord payment details not found.");
				}

				decimal amount = 0;
				if (paymentType == (int)PaymentType.Rent)
				{
					amount = house.Rent;
				}
				else if (paymentType == (int)PaymentType.Utility)
				{
					var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					if (int.TryParse(userIdStr, out var userId))
					{
						amount = await _context.UtilityReadings
							.Where(ur => ur.TenantId == userId && !ur.IsPaid)
							.SumAsync(ur => ur.TotalCost);
					}
				}

				var paymentDetails = new PaymentDetailsDto
				{
					PaymentMethod = "Pending",
					LAndlordDetails = landlordProfile,
					Amount = amount
				};

				return Json(paymentDetails);
			}
			catch (Exception ex)
			{
				return BadRequest($"Error: {ex.Message}");
			}
		}

		[HttpPost]
		[Route("ProcessPayment")]
		public async Task<IActionResult> ProcessPayment([FromBody] PaymentProcessingDto paymentProcessingDto)
		{
			try
			{
				var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
				{
					return Unauthorized("Invalid user ID");
				}

				using var transaction = await _context.Database.BeginTransactionAsync();
				try
				{
					var payment = new Payment
					{
						PaymentType = paymentProcessingDto.PaymentType,
						Amount = paymentProcessingDto.Amount,
						PaymentDate = DateTime.Now,
						PaymentMethod = paymentProcessingDto.PaymentMethod,
						PaymentStatus = "Pending",
						PaymentReference = GeneratePaymentReference(paymentProcessingDto.HouseId, userId),
						HouseId = paymentProcessingDto.HouseId,
						UserId = userId
					};

					_context.Payments.Add(payment);
					await _context.SaveChangesAsync();

					var paymentTransaction = new PaymentTransaction
					{
						PaymentId = payment.Id,
						TransactionReference = paymentProcessingDto.TransactionReference,
						TransactionStatus = "Pending",
						TransactionDate = DateTime.Now,
						PaymentProvider = paymentProcessingDto.PaymentMethod,
						Amount = paymentProcessingDto.Amount
					};

					_context.PaymentTransactions.Add(paymentTransaction);
					await _context.SaveChangesAsync();

					if (paymentProcessingDto.PaymentType == "Utility")
					{
						var unpaidUtilities = await _context.UtilityReadings
							.Where(ur => ur.TenantId == userId && !ur.IsPaid)
							.ToListAsync();

						foreach (var unpaidUtility in unpaidUtilities)
						{
							unpaidUtility.IsPaid = true;
							_context.Update(unpaidUtility);
						}
						await _context.SaveChangesAsync();
					}

					await transaction.CommitAsync();

					return Ok(new
					{
						paymentId = payment.Id,
						transactionId = paymentTransaction.Id,
						message = "Payment processed successfully"
					});
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return BadRequest($"Error processing payment: {ex.Message}");
				}
			}
			catch (Exception ex)
			{
				return BadRequest($"Error processing payment: {ex.Message}");
			}
		}
	}
}
