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
	public class Request
{
	public int Id { get; set; }
	public int TenantId { get; set; }
	public User Tenant { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public RequestPriority Priority { get; set; }
	public RequestStatus Status { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
	public DateTime? CompletedAt { get; set; }
	public string? LandlordNotes { get; set; }
	
	
}


public enum RequestStatus
{
	Pending,
	InProgress,
	Completed,
	Rejected,
	NeedMoreInfo
}

public enum RequestPriority
{
	Low,
	Medium,
	High,
	Emergency
}
}