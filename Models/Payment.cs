using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.Models
{
	public class Payment
	{
		public int Id { get; set; }
		public string PaymentType { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public string PaymentMethod { get; set; }
		public string PaymentStatus { get; set; }
		public string PaymentReference { get; set; }
		public string Description { get; set; }

		public int? HouseId { get; set; }
		public House House { get; set; }

		public int? UserId { get; set; }
		public User User { get; set; }
	}

	public enum PaymentMethod
	{
		Cash,
		BankTransfer,
		CreditCard,
		PayPal,
		Other
	}
	
	public enum PaymentType
	
	{
		Rent,
		Utility
	}

	public enum PaymentStatus
	{
		Pending,
		Completed,
		Failed,
		Refunded
	}
}