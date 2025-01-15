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
        public async Task<List<Bank>> LoadBanksAsync()
        {
            // Get the path to the banks.json file
               string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Data", "banks.json");

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The banks.json file was not found.");
            }

            // Read the JSON file content asynchronously
            string jsonData = await File.ReadAllTextAsync(filePath);

            // Deserialize the JSON into the root object first
            var bankRoot = JsonSerializer.Deserialize<BankRoot>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Allows case-insensitive property matching
            });

            // Return the list of banks
            return bankRoot?.Banks ?? new List<Bank>();
        }
    }
}