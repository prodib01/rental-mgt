namespace RentalManagementSystem.DTOs
{
    public class CreateLeaseDto
    {
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class LeaseDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string TenantEmail { get; set; }
        public string HouseNumber { get; set; }
        public string PropertyAddress { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
    }

}