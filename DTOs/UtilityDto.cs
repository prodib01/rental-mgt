using System.ComponentModel.DataAnnotations;

namespace RentalManagementSystem.DTOs

{
	public class UtilityDto

	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Cost { get; set; }
	}

	public class CreateUtilityDto

	{
		public string Name { get; set; }
		public int Cost { get; set; }
	}

	public class UtilityReadingDto
	{
		public int Id { get; set; }
		public string TenantName { get; set; }
		public string HouseNumber { get; set; }
		public DateTime ReadingDate { get; set; }
		public int PrevReading { get; set; }
		public int CurrentReading { get; set; }
		public int Consumption { get; set; }
		public int TotalCost { get; set; }
	}

	public class CreateUtilityReadingDto

	{
		public int UtilityId { get; set; }
		public int TenantId { get; set; }

		public int PrevReading { get; set; }
		public int CurrentReading { get; set; }
		public int Consumption { get; set; }
		public int TotalCost { get; set; }
	}
}