using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace RentalManagementSystem.Services

{
	public interface INotificationService
	{
		Task CreateNotificationAsync(string title, string message, NotificationType type, string targetUrl = null);
		Task<List<Notification>> GetUserNotificationsAsync(int userId);
		Task MarkAsReadAsync(int notificationId);
		Task DeleteNotificationAsync(int notificationId);
	}

	public class NotificationService : INotificationService

	{
		private readonly RentalManagementContext _context;
		private readonly IHubContext<NotificationHub> _hubContext;
		private readonly ILogger<NotificationService> _logger;

		public NotificationService(RentalManagementContext context, IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
		{
			_context = context;
			_hubContext = hubContext;
			_logger = logger;
		}

		public async Task CreateNotificationAsync(string title, string message, NotificationType type, string targetUrl = null)

		{
			var notification = new Notification

			{
				Title = title,
				Message = message,
				Type = type,
				CreatedAt = DateTime.UtcNow,
				IsRead = false,
				TargetUrl = targetUrl
			};

			_context.Notifications.Add(notification);
			await _context.SaveChangesAsync();

			await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
		}

		public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
		{
			return await _context.Notifications
				.Where(n => n.UserId == userId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}

		public async Task MarkAsReadAsync(int notificationId)
		{
			var notification = await _context.Notifications.FindAsync(notificationId);
			if (notification != null)
			{
				notification.IsRead = true;
				await _context.SaveChangesAsync();
			}
		}

		public async Task DeleteNotificationAsync(int notificationId)
		{
			var notification = await _context.Notifications.FindAsync(notificationId);
			if (notification != null)
			{
				_context.Notifications.Remove(notification);
				await _context.SaveChangesAsync();
			}
		}
	}

}