using EscalationPlatform.Shared.Pagination;
using EscalationPlatform.Shared.Result;
using EscalationService.Application.Common;
using EscalationService.Application.Common.Interfaces;
using EscalationService.Application.DTOs.SlaPolicy;
using EscalationService.Application.Mappings;
using EscalationService.Application.Services.Interfaces;
using EscalationService.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EscalationService.Application.Services.Implementation;

public sealed class SlaPolicyService : ISlaPolicyService
{
    private readonly IEscalationDbContext _db;
    private readonly IValidator<CreateSlaPolicyRequest> _createValidator;
    private readonly IValidator<UpdateSlaPolicyRequest> _updateValidator;
    private readonly ILogger<SlaPolicyService> _logger;

    public SlaPolicyService(
        IEscalationDbContext db,
        IValidator<CreateSlaPolicyRequest> createValidator,
        IValidator<UpdateSlaPolicyRequest> updateValidator,
        ILogger<SlaPolicyService> logger)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<Result<PagedResult<SlaPolicyResponse>>> GetAllAsync(PageParams? pageParams, CancellationToken ct)
    {
        var result = await _db.SlaPolicies
            .AsNoTracking()
            .OrderBy(p => p.MinPriorityLevel)
            .ToPageAsync(pageParams, ct);

        _logger.LogInformation("Retrieved {Count} sla policies (page {Page})", result.TotalCount, result.Page);

        var paged = new PagedResult<SlaPolicyResponse>(
            result.Items.Select(SlaPolicyMapper.ToResponse).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);

        return Result<PagedResult<SlaPolicyResponse>>.Success(paged);
    }

    public async Task<Result<SlaPolicyResponse>> GetByIdAsync(long id, CancellationToken ct)
    {
        var policy = await _db.SlaPolicies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (policy is null)
        {
            _logger.LogWarning("SlaPolicy {Id} not found", id);
            return Result<SlaPolicyResponse>.Failure(Error.NotFound<SlaPolicy>(id));
        }

        return Result<SlaPolicyResponse>.Success(SlaPolicyMapper.ToResponse(policy));
    }

    public async Task<Result> CreateAsync(CreateSlaPolicyRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result<SlaPolicyResponse>.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var duplicate = await _db.SlaPolicies.AnyAsync(p => p.Name == request.Name, ct);
        if (duplicate)
            return Result<SlaPolicyResponse>.Failure(Error.Duplicate(nameof(SlaPolicy.Name), request.Name));

        var policy = new SlaPolicy
        {
            Name = request.Name,
            Description = request.Description,
            MinPriorityLevel = request.MinPriorityLevel,
            ResponseTimeMinutes = request.ResponseTimeMinutes,
            ResolutionTimeMinutes = request.ResolutionTimeMinutes,
            IsActive = true
        };

        _db.SlaPolicies.Add(policy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created SlaPolicy {Id} with name '{Name}'", policy.Id, policy.Name);

        return Result.Success();
    }

    public async Task<Result> UpdateAsync(long id, UpdateSlaPolicyRequest request, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result<SlaPolicyResponse>.Failure(
                Error.ValidationFailed(validation.Errors.Select(e => e.ErrorMessage)));
        
        var policy = await _db.SlaPolicies.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (policy is null)
        {
            _logger.LogWarning("SlaPolicy {Id} not found", id);
            return Result<SlaPolicyResponse>.Failure(Error.NotFound<SlaPolicy>(id));
        }

        var duplicate = await _db.SlaPolicies.AnyAsync(p => p.Name == request.Name && p.Id != id, ct);
        if (duplicate)
            return Result<SlaPolicyResponse>.Failure(Error.Duplicate(nameof(SlaPolicy.Name), request.Name));

        policy.Name = request.Name;
        policy.Description = request.Description;
        policy.MinPriorityLevel = request.MinPriorityLevel;
        policy.ResponseTimeMinutes = request.ResponseTimeMinutes;
        policy.ResolutionTimeMinutes = request.ResolutionTimeMinutes;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated SlaPolicy {Id}", id);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct)
    {
        var policy = await _db.SlaPolicies.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (policy is null)
        {
            _logger.LogWarning("SlaPolicy {Id} not found", id);
            return Result.Failure(Error.NotFound<SlaPolicy>(id));
        }

        _db.SlaPolicies.Remove(policy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted SlaPolicy {Id}", id);

        return Result.Success();
    }

}
