using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Migrations;
using RentalManagementSystem.Models;

namespace RentalManagementSystem.ViewModels
{ 
public class ProfileViewModel

{
	public Profile Profile { get; set; }
	public List<Models.Bank> Banks { get; set; }
}	
}