using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class ScopeService : IScopeService
{
    private readonly IScopeRepository _scopeRepository;

    public ScopeService(IScopeRepository scopeRepository)
    {
        _scopeRepository = scopeRepository;
    }

    public async Task<Result<ScopeResponse>> CreateAsync(CreateScopeRequest request)
    {
        var existing = await _scopeRepository.GetByNameAsync(request.Name);
        if (existing is not null)
            return Result.Failure<ScopeResponse>(new Error("Scope.AlreadyExists", $"Scope '{request.Name}' already exists."));

        var scope = new Scope
        {
            Name = request.Name,
            Description = request.Description
        };

        await _scopeRepository.AddAsync(scope);

        return Result.Success(new ScopeResponse(scope.Id, scope.Name, scope.Description), "scope created successfully");
    }

    public async Task<Result<PagedResult<ScopeResponse>>> GetAllAsync(PaginationFilter filter)
    {
        var pagedScopes = await _scopeRepository.GetAllAsync(filter);
        var responseItems = pagedScopes.Items.Select(s => new ScopeResponse(s.Id, s.Name, s.Description));
        var response = new PagedResult<ScopeResponse>(responseItems, pagedScopes.TotalCount, pagedScopes.PageNumber, pagedScopes.PageSize);
        return Result.Success(response, "scopes retrieved successfully");
    }

    public async Task<Result<ScopeResponse>> GetByIdAsync(Guid id)
    {
        var scope = await _scopeRepository.GetByIdAsync(id);
        if (scope is null)
            return Result.Failure<ScopeResponse>(Error.ScopeNotFound);

        return Result.Success(new ScopeResponse(scope.Id, scope.Name, scope.Description), "scope retrieved successfully");
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateScopeRequest request)
    {
        var scope = await _scopeRepository.GetByIdAsync(id);
        if (scope is null)
            return Result.Failure(Error.ScopeNotFound);

        var existingWithSameName = await _scopeRepository.GetByNameAsync(request.Name);
        if (existingWithSameName is not null && existingWithSameName.Id != id)
            return Result.Failure(new Error("Scope.AlreadyExists", $"Scope '{request.Name}' already exists."));

        scope.Name = request.Name;
        scope.Description = request.Description;

        await _scopeRepository.UpdateAsync(scope);

        return Result.Success("scope updated successfully");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var scope = await _scopeRepository.GetByIdAsync(id);
        if (scope is null)
            return Result.Failure(Error.ScopeNotFound);

        await _scopeRepository.DeleteAsync(id);
        return Result.Success("scope deleted successfully");
    }
}
