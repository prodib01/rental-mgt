using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;
using iText.IO.Font.Constants;
using iText.Kernel.Font;




namespace RentalManagementSystem.Services
{
	public interface ILeaseDocumentService

	{
		Task<LeaseDocument> GenerateAndSaveDocument(Lease lease, string baseDirectory);
	}

	public class LeaseDocumentService : ILeaseDocumentService

	{
		private readonly RentalManagementContext _context;
		public LeaseDocumentService(RentalManagementContext context)

		{
			_context = context;
		}

		public async Task<LeaseDocument> GenerateAndSaveDocument(Lease lease, string baseDirectory)

		{
			var tenant = await _context.Users
			.Include(u => u.House)
			.ThenInclude(h => h.Property)
			.FirstOrDefaultAsync(u => u.Id == lease.TenantId);

			var landlord = await _context.Users
			.FirstOrDefaultAsync(u => u.Id == tenant.House.Property.UserId);

			// Create document directory if it doesn't exist
			var documentDirectory = Path.Combine(baseDirectory, "lease-documents");
			Directory.CreateDirectory(documentDirectory);

			// Generate unique filename
			string version = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			string fileName = $"lease-agreement-for-{lease.Tenant.FullName}";
			string filePath = Path.Combine(documentDirectory, fileName);

			//Generate PDF
			using (var writer = new PdfWriter(filePath))
			using (var pdf = new PdfDocument(writer))
			using (var document = new Document(pdf))
			{
				PdfFont boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
				PdfFont regularFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

				// Add header
				document.Add(new Paragraph("RESIDENTIAL LEASE AGREEMENT")
					.SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
					.SetFont(boldFont)
					.SetFontSize(20));

				// Add basic information
				document.Add(new Paragraph($"This lease agreement is made on {DateTime.UtcNow:MMMM dd, yyyy}"));
				document.Add(new Paragraph("\nBETWEEN"));

				// Landlord details
				document.Add(new Paragraph($"LANDLORD: {landlord.FullName}")
					.SetFont(boldFont));

				document.Add(new Paragraph($"Landlord Email: {landlord.Email}")
	.SetFont(regularFont));

				document.Add(new Paragraph($"Landlord Phone: {landlord.PhoneNumber}")
				.SetFont(regularFont));

				// Tenant details
				document.Add(new Paragraph($"TENANT: {tenant.FullName}")
					.SetFont(boldFont));

				document.Add(new Paragraph($"Tenant Email: {tenant.Email}")
.SetFont(regularFont));

				document.Add(new Paragraph($"Tenant Phone: {tenant.PhoneNumber}")
					.SetFont(regularFont));

				// Property details
				document.Add(new Paragraph("PROPERTY DETAILS")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph($"Address: {tenant.House.Property.Address}")
					.SetFont(regularFont));
				document.Add(new Paragraph($"House Number: {tenant.House.HouseNumber}")
					.SetFont(regularFont));

				// Lease terms
				document.Add(new Paragraph("LEASE TERMS")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph($"Start Date: {lease.StartDate:MMMM dd, yyyy}")
					.SetFont(regularFont));
				document.Add(new Paragraph($"End Date: {lease.EndDate:MMMM dd, yyyy}")
					.SetFont(regularFont));
				document.Add(new Paragraph($"Monthly Rent: {tenant.House.Rent:C}")
					.SetFont(regularFont));

				// Utilities and responsibilities
				document.Add(new Paragraph("UTILITIES AND RESPONSIBILITIES")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph("The tenant is responsible for the following utilities: Electricity, Water, Gas, etc.")
					.SetFont(regularFont));
				document.Add(new Paragraph("The landlord is responsible for the following: Property maintenance, Repairs, etc.")
					.SetFont(regularFont));

				// Maintenance and Repairs
				document.Add(new Paragraph("MAINTENANCE AND REPAIRS")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph("The tenant agrees to maintain the property in a clean and sanitary condition.")
					.SetFont(regularFont));
				document.Add(new Paragraph("The landlord agrees to repair any structural damage to the property, including plumbing and electrical systems.")
					.SetFont(regularFont));

				// Rules and Regulations
				document.Add(new Paragraph("RULES AND REGULATIONS")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph("No pets are allowed on the premises without prior written consent from the landlord.")
					.SetFont(regularFont));
				document.Add(new Paragraph("The tenant agrees to not engage in illegal activities on the property.")
					.SetFont(regularFont));
				document.Add(new Paragraph("The tenant agrees to comply with all local laws and property regulations.")
					.SetFont(regularFont));

				// Insurance
				document.Add(new Paragraph("INSURANCE REQUIREMENTS")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph("The tenant is required to maintain renters insurance covering personal property damage and liability.")
					.SetFont(regularFont));
				document.Add(new Paragraph("The landlord is required to maintain insurance covering the property and liability.")
					.SetFont(regularFont));

				// Termination Clause
				document.Add(new Paragraph("TERMINATION CLAUSE")
					.SetFont(boldFont)
					.SetMarginTop(20));
				document.Add(new Paragraph("Either party may terminate the lease with a written notice 30 days in advance.")
					.SetFont(regularFont));
				document.Add(new Paragraph("The tenant must return the property in the same condition as it was received, except for normal wear and tear.")
					.SetFont(regularFont));

				// Add signature spaces
				document.Add(new Paragraph("\n\n"));
				document.Add(new Paragraph("SIGNATURES")
					.SetFont(boldFont)
					.SetMarginTop(40));
				document.Add(new Paragraph("_______________________")
					.SetMarginTop(20));
				document.Add(new Paragraph($"Landlord: {landlord.FullName}"));

				document.Add(new Paragraph("_______________________")
					.SetMarginTop(20));
				document.Add(new Paragraph($"Tenant: {tenant.FullName}"));
			}

			// Create lease document record
			var leaseDocument = new LeaseDocument
			{
				LeaseId = lease.Id,
				DocumentPath = filePath,
				DocumentName = fileName,
				GeneratedAt = DateTime.UtcNow,
				Version = version
			};


			_context.LeaseDocuments.Add(leaseDocument);
			await _context.SaveChangesAsync();

			return leaseDocument;
		}
	}
}
