using RentalManagementSystem.Models;

namespace RentalManagementSystem.DTOs
{ 
	public class PaymentProcessingDto
	
	{
		public string PaymentType { get; set; }
		public string PaymentMethod { get; set; }
		public decimal Amount { get; set; }
		public int HouseId { get; set; }
		public string TransactionReference { get; set; }
	}
	
	public class PaymentDetailsDto
	
	{
		public string PaymentMethod { get; set; }
		public Profile LAndlordDetails { get; set; }	
		public decimal Amount { get; set; }
	}
}