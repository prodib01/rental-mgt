using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RentalManagementSystem.Models
{
    public class FinancialReport
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public List<Payment> Payments { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }

    public class OccupancyReport
    {
        public int Id { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int TotalUnits { get; set; }
        public int OccupiedUnits { get; set; }
        public int VacantUnits { get; set; }
        public decimal OccupancyRate { get; set; }
        public List<House> UnitStatus { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }

    public class MaintenanceReport
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRequests { get; set; }
        public int ResolvedRequests { get; set; }
        public int PendingRequests { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public List<Request> Requests { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }

    public class LeaseReport
    {
        public int Id { get; set; }
        public DateTime GeneratedDate { get; set; }
        public int ActiveLeases { get; set; }
        public int ExpiringLeases { get; set; }
        public int RenewedLeases { get; set; }
        public List<Lease> LeaseDetails { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}
