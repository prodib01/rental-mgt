using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.DTOs;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementSystem.Services;
using RentalManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace RentalManagementSystem.Controllers

{
	[Authorize]
	[Route("api/[controller]")]
	public class NotificationController : ControllerBase

	{
		private readonly INotificationService _notificationService;

		public NotificationController(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		[HttpPost]
		public async Task<IActionResult> CreateNotification([FromBody] NotificationRequest request)

		{
			await _notificationService.CreateNotificationAsync(
				request.Title,
				request.Message,
				request.Type,
				request.TargetUrl);

			return Ok();
		}
		
		
		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetUserNotifications(int userId)
		
		{
			var notifications = await _notificationService.GetUserNotificationsAsync(userId);
			return Ok(notifications);
		}

		[HttpPut("{id}/read")]
		public async Task<IActionResult> MarkAsRead(int id)
		{
			await _notificationService.MarkAsReadAsync(id);
			return Ok();
		}
		
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteNotification(int id)
		{
			await _notificationService.DeleteNotificationAsync(id);
			return Ok();
		}
	}
}
