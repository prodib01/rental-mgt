using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.Models

{
	public class PaymentTransaction
	
	{
		public int Id { get; set; }
		public int PaymentId { get; set; }
		public Payment Payment { get; set; }
		public string TransactionReference { get; set; }	
		public string TransactionStatus { get; set; }
		public DateTime TransactionDate { get; set; }
		public string PaymentProvider { get; set; }
		public string ProviderReference { get; set; }
		public string RecieptDetails { get; set; }
		public decimal Amount { get; set; }
	}
}