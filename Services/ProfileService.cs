using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentalManagementSystem.DTOs;

namespace RentalManagementSystem.Services
{
	public interface IProfileService
	{
		Task<Profile> CreateProfileAsync(CreateProfileDTO profileDTO, int landlordId);
		Task<Profile> GetProfileByLandlordIdAsync(int landlordId);
		Task<Profile> UpdateProfileAsync(CreateProfileDTO profileDTO, int landlordId);
	}

	public class ProfileService : IProfileService
	{
		private readonly RentalManagementContext _context;
		private readonly IBankService _bankService;

		public ProfileService(RentalManagementContext context, IBankService bankService)
		{
			_context = context;
			_bankService = bankService;
		}

		public async Task<Profile> CreateProfileAsync(CreateProfileDTO profileDTO, int landlordId)
		{
			// Get the banks
			var banks = await _bankService.LoadBanksAsync();

			// Find the bank by ID
			var bank = banks.FirstOrDefault(b => b.Id.ToString() == profileDTO.Bank);
			if (bank == null)
			{
				throw new ArgumentException("Invalid bank selected");
			}

			var profile = new Profile
			{
				LandlordId = landlordId,
				Bank = bank.Name,  // Store the bank name
				AccountNumber = profileDTO.AccountNumber,
				AccountHolderName = profileDTO.AccountHolderName,
				NumberForPayments = profileDTO.NumberForPayments
			};

			_context.Profiles.Add(profile);
			await _context.SaveChangesAsync();

			return profile;
		}

		public async Task<Profile> GetProfileByLandlordIdAsync(int landlordId)
		{
			return await _context.Profiles
				.FirstOrDefaultAsync(p => p.LandlordId == landlordId);
		}

		public async Task<Profile> UpdateProfileAsync(CreateProfileDTO profileDTO, int landlordId)

		{
			var profile = await _context.Profiles
				.FirstOrDefaultAsync(p => p.LandlordId == landlordId);

			if (profile == null)

			{
				throw new ArgumentException("Profile not found");
			}

			var banks = await _bankService.LoadBanksAsync();
			if (!banks.Any(b => b.Name == profileDTO.Bank))
			{
				throw new ArgumentException("Invalid bank selected");
			}

			profile.Bank = profileDTO.Bank;
			profile.AccountNumber = profileDTO.AccountNumber;
			profile.AccountHolderName = profileDTO.AccountHolderName;
			profile.NumberForPayments = profileDTO.NumberForPayments;

			await _context.SaveChangesAsync();
			return profile;
		}
	}
}
