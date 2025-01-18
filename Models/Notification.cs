using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementSystem.Models

{
	public class Notification
	
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public User User { get; set; }
		public string Title { get; set; }
		public string Message { get; set; }
		public NotificationType Type { get; set; }
		public DateTime CreatedAt { get; set; }
		public bool IsRead { get; set; }
		public string TargetUrl { get; set; }
	}
	
	public enum NotificationType
	
	{
		Info,
		Warning,
		Error,
		Success
	}
}