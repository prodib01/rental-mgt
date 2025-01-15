using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Migrations;

namespace RentalManagementSystem.ViewModels
{ 
public class ProfileViewModel

{
	public CreateProfileDTO Profile { get; set; }
	public List<Banks> Banks { get; set; }
}	
}