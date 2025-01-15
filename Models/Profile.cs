using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.Models
{
	public class Profile
	{
		[Key]
		public int Id { get; set; }
		public int LandlordId { get; set; }
		public User Landlord { get; set; }
		public string Bank { get; set; }
		public string AccountNumber { get; set; }
		public string AccountHolderName { get; set; }
		public string NumberForPayments { get; set; }
		
	}
}