using System;
using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.DTOs
{
public class CreatePaymentDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public int? HouseId { get; set; }
    public int? UserId { get; set; }
}


	public class PaymentDto : CreatePaymentDto
	{
		public int Id { get; set; }
	}
}