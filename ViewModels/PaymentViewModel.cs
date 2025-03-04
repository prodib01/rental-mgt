using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Models; // Reference for PaymentStatus and PaymentMethod

namespace RentalManagementSystem.ViewModels
{
	public class PaymentListItemViewModel
	{
		public int Id { get; set; }
		public decimal Amount { get; set; }
		public string PaymentType { get; set; }
		public DateTime PaymentDate { get; set; }
		public string PaymentMethod { get; set; }
		public string PaymentStatus { get; set; }
		public string PaymentReference { get; set; }
		public int? HouseId { get; set; }
		public int? UserId { get; set; }
		public House House { get; set; }
		public User User { get; set; }
	}

	public class PaymentViewModel
	{
		public IEnumerable<PaymentListItemViewModel> Payments { get; set; }
		public IEnumerable<SelectListItem> Houses { get; set; }  
		public IEnumerable<SelectListItem> Users { get; set; }   
		public int? HouseId { get; set; }  
		public int? UserId { get; set; }  
		public string StatusMessage { get; set; }
	}

}
