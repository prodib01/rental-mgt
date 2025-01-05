using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementSystem.Models
{
public class Lease

{
	public int Id { get; set; }
	public int TenantId { get; set; }
	public User Tenant { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? UpdatedAt { get; set; }
}
}