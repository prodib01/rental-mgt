using System.ComponentModel.DataAnnotations;
using RentalManagementSystem.Models; 

namespace RentalManagementSystem.DTOs


{
	public class RequestDto
{
	public int Id { get; set; }
	
	public int TenantId { get; set; }
	public string TenantName { get; set; }
	public string HouseNumber { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public RequestPriority Priority { get; set; }
	public RequestStatus Status { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public DateTime? CompletedAt { get; set; }
	public string? LandlordNotes { get; set; }
}

public class CreateRequestDto
{
	public int TenantId { get; set; }
	public int PropertyId { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public RequestPriority Priority { get; set; }
}

// DTOs/UpdateRequestDto.cs
public class UpdateRequestDto
{
	public RequestStatus Status { get; set; }
	public string? LandlordNotes { get; set; }
}
}