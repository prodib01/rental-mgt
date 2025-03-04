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
	[Authorize]
	[Route("Landlord/Payments")]
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
					var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (!int.TryParse(userIdStr, out var userId))
		{
			return Unauthorized("Invalid User ID.");
		}
				
				var query = _context.Payments
				.Where(p => p.House.Property.UserId == userId);
			

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
					PaymentType = p.PaymentType,
					PaymentMethod = p.PaymentMethod.ToString(), 
					PaymentStatus = p.PaymentStatus.ToString(), 
					PaymentReference = p.PaymentReference,
					HouseId = p.HouseId,
					UserId = p.UserId,
					House = p.House,
					User = p.User,
				})
				.ToListAsync();

			var houses = await _context.Houses
				.Select(h => new SelectListItem
				{
					Value = h.Id.ToString(),
					Text = h.HouseNumber
				})
				.ToListAsync();

			var users = await _context.Users

				.Select(u => new SelectListItem
				{
					Value = u.Id.ToString(),
					Text = u.Email
				})
				.ToListAsync();

			var viewModel = new PaymentViewModel
			{
				Payments = payments,
				Houses = houses,  
				Users = users,    
				StatusMessage = totalPayments > 0 ? $"{totalPayments} payments found." : "No payments found."
			};


			return View("~/Views/Landlord/Payments.cshtml", viewModel);
		}
		
		[HttpGet]
		[Route("Receipt/{id}")]
		public async Task<IActionResult> Receipt(int id)
		{
			var payment = await _context.Payments
				.Include(p => p.House)
				.Include(p => p.User)
				.FirstOrDefaultAsync(p => p.Id == id);
    
			if (payment == null)
			{
				return NotFound();
			}

			return View("~/Views/Landlord/Receipt.cshtml", payment);
		}
	}
}
