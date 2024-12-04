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
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            // Fetch user with house information
            var user = await _context.Users
                .Include(u => u.House)  // Make sure House is included
                .FirstOrDefaultAsync(u => u.Id == userId);

            // Prepare the query for payments
            var query = _context.Payments.AsQueryable();
            if (userRole == "Tenant")
            {
                query = query.Where(p => p.UserId == userId);
            }

            var totalPayments = await query.CountAsync();

            var payments = await query
                .Include(p => p.House)
                .Include(p => p.User)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(page => new PaymentListItemViewModel
                {
                    Id = page.Id,
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

            // Get all houses (you might want to filter this based on your requirements)
            var houses = await _context.Houses
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = h.HouseNumber
                })
                .ToListAsync();

            var viewModel = new PaymentViewModel
            {
                Payments = payments,
                Houses = houses,
                HouseId = user?.HouseId,  // Set the HouseId from the user
                UserId = userId
            };

            return View("~/Views/Tenant/Payments.cshtml", viewModel);
        }

        [HttpPost]
        [Route("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Amount,PaymentDate,PaymentMethod,PaymentStatus,PaymentReference,Description,HouseId,UserId")] CreatePaymentDto paymentDto, int page = 1, int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (ModelState.IsValid)
            {
                var payment = new Payment
                {
                    Amount = paymentDto.Amount,
                    PaymentDate = paymentDto.PaymentDate,
                    PaymentMethod = paymentDto.PaymentMethod.ToString(),
                    PaymentStatus = paymentDto.PaymentStatus.ToString(),
                    PaymentReference = GeneratePaymentReference(paymentDto.HouseId, paymentDto.UserId),
                    Description = paymentDto.Description,
                    HouseId = user.HouseId,
                    UserId = userId
                };

                _context.Add(payment);
                await _context.SaveChangesAsync();

                // Redirect back to the Payment page with pagination parameters
                return RedirectToAction(nameof(Payment), new { page = page, pageSize = pageSize });
            }

            // If ModelState is not valid, redirect back to the Payment page as a fallback
            return RedirectToAction(nameof(Payment), new { page = page, pageSize = pageSize });
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
    }
}
