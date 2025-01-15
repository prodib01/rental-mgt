using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using System.Security.Claims;
using System.Text.Json;

namespace RentalManagementSystem.Services
{
	public interface IBankService
	{
		Task<List<Bank>> LoadBanksAsync();
	}

	public class BankService : IBankService
	{
		private readonly RentalManagementContext _context;

		public BankService(RentalManagementContext context)
		{
			_context = context;
		}

		public async Task<List<Bank>> LoadBanksAsync()
		{
			// First check if banks exist in the database
			if (!await _context.Banks.AnyAsync())
			{
				// If no banks in database, load from JSON and save to database
				var banksFromJson = await LoadBanksFromJsonAsync();
				await SeedBanksToDatabase(banksFromJson);
			}

			// Return banks from database
			return await _context.Banks.Include(b => b.Contact_Info).ToListAsync();
		}

		private async Task<List<Bank>> LoadBanksFromJsonAsync()
		{
			string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "banks.json");

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("The banks.json file was not found.");
			}

			string jsonData = await File.ReadAllTextAsync(filePath);
			var bankRoot = JsonSerializer.Deserialize<BankRoot>(jsonData, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			return bankRoot?.Banks ?? new List<Bank>();
		}

		private async Task SeedBanksToDatabase(List<Bank> banks)
		{
			foreach (var bank in banks)
			{
				// Create ContactInfo first
				var contactInfo = new ContactInfo
				{
					Phone = bank.Contact_Info.Phone,
					Email = bank.Contact_Info.Email
				};

				var newBank = new Bank
				{
					Name = bank.Name,
					Head_Office = bank.Head_Office,
					Website = bank.Website,
					Year_Of_Establishment = bank.Year_Of_Establishment,
					Contact_Info = contactInfo
				};

				_context.Banks.Add(newBank);
			}

			await _context.SaveChangesAsync();
		}
	}
}