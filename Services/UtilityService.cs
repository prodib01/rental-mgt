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
	public interface IUtilityService
	{
		Task<List<UtilityDto>> GetAllUtilitiesAsync();
		Task<Utility> GetUtilityByIdAsync(int id);
		Task<List<UtilityReadingDto>> GetReadingsAsync(int utilityId);
		Task<UtilityReading> GetLastReadingAsync(int utilityId, int tenantId);
		Task<UtilityReading> GetReadingByIdAsync(int id);
		Task CreateUtilityAsync(CreateUtilityDto dto);
		Task AddReadingAsync(int utilityId, CreateUtilityReadingDto dto);
		Task UpdateReadingAsync(int id, CreateUtilityReadingDto dto);
	}

	public class UtilityService : IUtilityService
	{
		private readonly RentalManagementContext _context;

		public UtilityService(RentalManagementContext context)
		{
			_context = context;
		}

		public async Task<List<UtilityDto>> GetAllUtilitiesAsync()
		{
			return await _context.Utilities
				.Select(u => new UtilityDto
				{
					Id = u.Id,
					Name = u.Name,
					Cost = u.Cost
				}).ToListAsync();
		}

		public async Task<Utility> GetUtilityByIdAsync(int id)
		{
			return await _context.Utilities
				.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<List<UtilityReadingDto>> GetReadingsAsync(int utilityId)
		{
			return await _context.UtilityReadings
				.Where(r => r.UtilityId == utilityId)
				.Include(r => r.Tenant)
				.OrderByDescending(r => r.ReadingDate)
				.Select(r => new UtilityReadingDto
				{
					TenantName = r.Tenant.FullName,
					HouseNumber = r.Tenant.House.HouseNumber,
					ReadingDate = r.ReadingDate,
					PrevReading = r.PrevReading,
					CurrentReading = r.CurrentReading,
					Consumption = r.Consumption,
					TotalCost = r.TotalCost
				}).ToListAsync();
		}

		public async Task<UtilityReading> GetLastReadingAsync(int utilityId, int tenantId)
		{
			return await _context.UtilityReadings
				.Where(r => r.UtilityId == utilityId && r.TenantId == tenantId)
				.OrderByDescending(r => r.ReadingDate)
				.FirstOrDefaultAsync();
		}

		public async Task CreateUtilityAsync(CreateUtilityDto dto)
		{
			var utility = new Utility
			{
				Name = dto.Name,
				Cost = dto.Cost
			};

			_context.Utilities.Add(utility);
			await _context.SaveChangesAsync();
		}

		public async Task AddReadingAsync(int utilityId, CreateUtilityReadingDto dto)
		{
			var utility = await GetUtilityByIdAsync(utilityId);
			int consumption = dto.CurrentReading - dto.PrevReading;

			var reading = new UtilityReading
			{
				UtilityId = utilityId,
				TenantId = dto.TenantId,
				ReadingDate = DateTime.Now,
				PrevReading = dto.PrevReading,
				CurrentReading = dto.CurrentReading,
				Consumption = consumption,
				TotalCost = consumption * utility.Cost
			};

			_context.UtilityReadings.Add(reading);
			await _context.SaveChangesAsync();
		}

		public async Task<UtilityReading> GetReadingByIdAsync(int id)
		{
			return await _context.UtilityReadings
				.Include(r => r.Utility)
				.Include(r => r.Tenant)
				.FirstOrDefaultAsync(r => r.Id == id);
		}

		public async Task UpdateReadingAsync(int id, CreateUtilityReadingDto dto)
		{
			var reading = await GetReadingByIdAsync(id);
			if (reading == null) throw new KeyNotFoundException("Reading not found");

			int consumption = dto.CurrentReading - dto.PrevReading;

			reading.TenantId = dto.TenantId;
			reading.PrevReading = dto.PrevReading;
			reading.CurrentReading = dto.CurrentReading;
			reading.Consumption = consumption;
			reading.TotalCost = consumption * reading.Utility.Cost;

			await _context.SaveChangesAsync();
		}
	}
}