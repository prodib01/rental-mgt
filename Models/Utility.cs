using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.Models
{ 
	public class Utility
	
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Cost { get; set; }
	}
	
	
	public class UtilityReading
	
	{
		public int Id { get; set; }
		public int UtilityId { get; set; }
		public Utility Utility { get; set; }
		
		public int TenantId { get; set; }
		public User Tenant { get; set; }
		public DateTime ReadingDate { get; set; }
		public int PrevReading { get; set; }
		public int CurrentReading { get; set; }
		public int Consumption { get; set; }
		public int TotalCost { get; set; }
	}
}