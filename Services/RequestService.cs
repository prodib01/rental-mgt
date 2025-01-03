using Microsoft.EntityFrameworkCore;
using RentalManagementSystem.Models;
using RentalManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RentalManagementSystem.DTOs;
using AutoMapper;



namespace RentalManagementSystem.Services

{
public interface IRequestService
{
	Task<IEnumerable<RequestDto>> GetAllRequestsAsync(int landlordId);
	Task<RequestDto> GetRequestByIdAsync(int requestId);
	Task<RequestDto> CreateRequestAsync(CreateRequestDto dto);
	Task<RequestDto> UpdateRequestStatusAsync(int requestId, UpdateRequestDto updateDto);
	Task DeleteRequestAsync(int requestId);
}

public class RequestService : IRequestService
{
	private readonly RentalManagementContext _context;
	private readonly IMapper _mapper;

	public RequestService(RentalManagementContext context, IMapper mapper)
	{
		_context = context;
		_mapper = mapper;
	}

	public async Task<IEnumerable<RequestDto>> GetAllRequestsAsync(int landlordId)
	{
		var requests = await _context.Requests
			.Include(r => r.Tenant)
			.Where(r => r.Tenant.House.Property.UserId == landlordId)
			.OrderByDescending(r => r.CreatedAt)
			.ToListAsync();

		return _mapper.Map<IEnumerable<RequestDto>>(requests);
	}

public async Task<RequestDto> GetRequestByIdAsync(int requestId)
{
    var request = await _context.Requests
        .Include(r => r.Tenant)
        .FirstOrDefaultAsync(r => r.Id == requestId);

    if (request == null) 
        throw new InvalidOperationException("Request not found");

    return _mapper.Map<RequestDto>(request);
}

	public async Task<RequestDto> CreateRequestAsync(CreateRequestDto dto)
	{
		var request = _mapper.Map<Request>(dto);
		request.CreatedAt = DateTime.UtcNow;

		_context.Requests.Add(request);
		await _context.SaveChangesAsync();

		return _mapper.Map<RequestDto>(request);
	}

public async Task<RequestDto> UpdateRequestStatusAsync(int requestId, UpdateRequestDto updateDto)
{
    var request = await _context.Requests.FindAsync(requestId);
    if (request == null) 
        throw new InvalidOperationException("Request not found");

    request.Status = updateDto.Status;
    request.LandlordNotes = updateDto.LandlordNotes;
    request.UpdatedAt = DateTime.UtcNow;

    if (updateDto.Status == RequestStatus.Completed)
    {
        request.CompletedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
    return _mapper.Map<RequestDto>(request);
}

public async Task DeleteRequestAsync(int requestId)
{
    var request = await _context.Requests.FindAsync(requestId);
    if (request == null) 
        throw new InvalidOperationException("Request not found");

    _context.Requests.Remove(request);
    await _context.SaveChangesAsync();
}
}

}