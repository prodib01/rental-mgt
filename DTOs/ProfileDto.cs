namespace RentalManagementSystem.DTOs
{
    public class CreateProfileDTO
    {
        public string Bank { get; set; }
        public string AccountNumber { get; set; }
        public string AccountHolderName { get; set; }
        public string NumberForPayments { get; set; }
    }
}