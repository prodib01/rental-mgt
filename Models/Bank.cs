using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.Models
{
	public class Bank
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; }
		public string Head_Office { get; set; }
		public string Website { get; set; }
		public int Year_Of_Establishment { get; set; }
		public ContactInfo Contact_Info { get; set; }
	}

	public class ContactInfo
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public int BankId { get; set; }  // Add this foreign key property
		public Bank Bank { get; set; }    // Navigation property
	}

	public class BankRoot

	{
		public List<Bank> Banks { get; set; }
	}
}