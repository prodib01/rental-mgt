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
    [Route("Landlord/Payments")]
    // [Authorize(Roles = "Landlord")]
    public class PaymentController : Controller
    {
        private readonly RentalManagementContext _context;

        public PaymentController(RentalManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Payment(int page = 1, int pageSize = 10)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");
            var userRole = User.FindFirst("Role")?.Value;

            IQueryable<Payment> query = _context.Payments;

            if (userRole == "Landlord")
            {
                // Filter payments related to the landlord's properties/houses
                query = query.Where(p =>
                    (p.House != null && p.House.Property.UserId == userId) ||
                    (p.User != null && p.User.House != null && p.User.House.Property.UserId == userId)
                );
            }

            var totalPayments = await query.CountAsync();

            var payments = await query
                .Include(p => p.House)
                .Include(p => p.User)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentListItemViewModel
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod.ToString(), // Convert enum to string
                    PaymentStatus = p.PaymentStatus.ToString(), // Convert enum to string
                    PaymentReference = p.PaymentReference,
                    HouseId = p.HouseId,
                    UserId = p.UserId,
                    House = p.House,
                    User = p.User,
                })
                .ToListAsync();

            var houses = await _context.Houses
                .Where(h => h.Property.UserId == userId)
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = h.HouseNumber
                })
                .ToListAsync();

            var users = await _context.Users
                .Where(u => u.HouseId.HasValue &&
                            u.House.Property.UserId == userId)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Email
                })
                .ToListAsync();

            var viewModel = new PaymentViewModel
            {
                Payments = payments,
                Houses = houses,  // Use the `houses` variable here
                Users = users,    // Use the `users` variable here
                StatusMessage = totalPayments > 0 ? $"{totalPayments} payments found." : "No payments found."
            };


            return View("~/Views/Landlord/Payments.cshtml", viewModel);
        }

        [HttpPost]
        [Route("Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([Bind("Amount,PaymentDate,PaymentMethod,PaymentStatus,PaymentReference,Description,HouseId,UserId")] CreatePaymentDto paymentDto)
        {
            var userId = int.Parse(User.FindFirst("uid")?.Value ?? "0");

            if (ModelState.IsValid)
            {
                var payment = new Payment
                {
                    Amount = paymentDto.Amount,
                    PaymentDate = paymentDto.PaymentDate,
                    PaymentMethod = paymentDto.PaymentMethod.ToString(), // Convert enum to string
                    PaymentStatus = paymentDto.PaymentStatus.ToString(), // Convert enum to string
                    PaymentReference = GeneratePaymentReference(paymentDto.HouseId, paymentDto.UserId), // Generate reference
                    Description = paymentDto.Description,
                    UserId = userId // Assign the user ID
                };

                _context.Add(payment);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Payment));
            }

            return RedirectToAction(nameof(Payment));
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
                payment.PaymentMethod = paymentDto.PaymentMethod.ToString(); // Convert enum to string
                payment.PaymentStatus = paymentDto.PaymentStatus.ToString(); // Convert enum to string
                payment.PaymentReference = GeneratePaymentReference(paymentDto.HouseId, paymentDto.UserId); // Generate reference
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
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // For uniqueness based on timestamp
                return $"{houseId}-{userId}-{timestamp}"; // Example: "12-34-20231205094530"
            }

            return "REF-UNKNOWN"; // Fallback reference if IDs are not provided
        }
    }
}
