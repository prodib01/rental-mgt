using System;
using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.Models;


namespace RentalManagementSystem.DTOs
{
    public class CreatePaymentDto
    {
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; } // Use enum type
        public PaymentStatus PaymentStatus { get; set; } // Use enum type
        public string PaymentReference { get; set; }
        public string Description { get; set; }
        public int HouseId { get; set; }  // Add HouseId property
        public int UserId { get; set; }
    }


    public class PaymentDto : CreatePaymentDto
    {
        public int Id { get; set; }
    }
}