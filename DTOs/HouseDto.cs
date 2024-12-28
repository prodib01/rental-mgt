using System.ComponentModel.DataAnnotations;

namespace RentalManagementSystem.DTOs
{
    public class HouseDto
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; }
        public decimal Rent { get; set; }
        public bool IsOccupied { get; set; }
        public int PropertyId { get; set; }
        public int UserId { get; set; }
    }

    public class CreateHouseDto
    {
        [Required]
        public decimal Rent { get; set; }

        [Required]
        public int PropertyId { get; set; }
    }
}
