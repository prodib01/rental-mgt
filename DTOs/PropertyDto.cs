using System.ComponentModel.DataAnnotations;

namespace RentalManagementSystem.DTOs
{
    public class PropertyDto
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
    }

    public class CreatePropertyDto
    {
        [Required]
        public string Address { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
