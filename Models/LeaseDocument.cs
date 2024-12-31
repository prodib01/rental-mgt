using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.Models
{
	public class LeaseDocument
	{
		public int Id { get; set; }
		public int LeaseId { get; set; }
		public Lease Lease { get; set; }
		public string DocumentPath { get; set; }
		public string DocumentName { get; set; }
		public DateTime GeneratedAt { get; set; }
		public string Version { get; set; }


	}
}