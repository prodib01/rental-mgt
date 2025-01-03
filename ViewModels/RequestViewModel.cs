using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementSystem.Models; 

namespace RentalManagementSystem.ViewModels
{
	public class RequestViewModel
	{
		public int Id { get; set; }
		public string TenantName { get; set; }
		public string HouseNumber { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public RequestPriority Priority { get; set; }
		public RequestStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public string TimeAgo => CalculateTimeAgo(CreatedAt);
		public string? LandlordNotes { get; set; }

		private string CalculateTimeAgo(DateTime date)
		{
			var timeSpan = DateTime.UtcNow - date;
			if (timeSpan.Days > 0) return $"{timeSpan.Days} days ago";
			if (timeSpan.Hours > 0) return $"{timeSpan.Hours} hours ago";
			return $"{timeSpan.Minutes} minutes ago";
		}
	}

	public class RequestListViewModel
	{
		public IEnumerable<RequestViewModel> Requests { get; set; }
		public RequestStatus? FilterStatus { get; set; }
		public string? SearchTerm { get; set; }
		public int TotalRequests { get; set; }
		public int PendingRequests { get; set; }
	}
}
