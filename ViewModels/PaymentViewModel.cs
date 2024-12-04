using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementSystem.DTOs;
using RentalManagementSystem.Models; // Reference for PaymentStatus and PaymentMethod

namespace RentalManagementSystem.ViewModels
{
    public class PaymentListItemViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentReference { get; set; }
        public int? HouseId { get; set; }
        public int? UserId { get; set; }
        public House House { get; set; }
        public User User { get; set; }
    }

    public class PaymentViewModel
    {
        public IEnumerable<PaymentListItemViewModel> Payments { get; set; }
        public IEnumerable<SelectListItem> Houses { get; set; }  // Changed from IEnumerable<House>
        public IEnumerable<SelectListItem> Users { get; set; }   // Changed from IEnumerable<User>
        public int? HouseId { get; set; }  // Added for form binding
        public int? UserId { get; set; }   // Added for form binding
        public string StatusMessage { get; set; }
    }





}
