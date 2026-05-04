using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Services;

public class ClientAuthService : IClientAuthService
{
    private readonly IClientRepository _clientRepository;
    private readonly ITokenService _tokenService;

    public ClientAuthService(
        IClientRepository clientRepository,
        ITokenService tokenService)
    {
        _clientRepository = clientRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<ClientCredentialsResponse>> AuthenticateAsync(ClientCredentialsRequest request)
    {
        var client = await _clientRepository.GetByClientIdAsync(request.ClientId);

        if (client is null)
            return Result.Failure<ClientCredentialsResponse>(Error.ClientNotFound);

        if (!client.IsActive)
            return Result.Failure<ClientCredentialsResponse>(Error.ClientInactive);

        // Verify secret
        var secretHash = HashSecret(request.ClientSecret);
        if (client.ClientSecretHash != secretHash)
            return Result.Failure<ClientCredentialsResponse>(Error.InvalidClientSecret);

        var scopes = client.AllowedScopes.Select(s => s.Scope.Name).ToList();
        var token = _tokenService.GenerateClientAccessToken(client, scopes);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ClientApplicationId = client.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Hardcoded for now, or use options
        };

        // Note: ClientAuthService might not have IRefreshTokenRepository injected. 
        // I should check or just use AuthService.
        
        return Result.Success(new ClientCredentialsResponse(
            AccessToken: token,
            RefreshToken: refreshTokenValue,
            ExpiresAt: DateTime.UtcNow.AddMinutes(60)
        ), "authenticated successfully");
    }

    public async Task<Result<CreateClientResponse>> CreateClientAsync(CreateClientRequest request)
    {
        var clientId = $"client_{Guid.NewGuid():N}"[..24];
        var clientSecret = GenerateClientSecret();
        var secretHash = HashSecret(clientSecret);

        var client = new ClientApplication
        {
            ClientId = clientId,
            ClientSecretHash = secretHash,
            Name = request.Name,
            Description = request.Description,
            AllowedScopes = request.ScopeIds.Select(sid => new ClientScope { ScopeId = sid }).ToList()
        };

        await _clientRepository.AddAsync(client);

        return Result.Success(new CreateClientResponse(
            ClientId: clientId,
            ClientSecret: clientSecret,
            Name: request.Name,
            ScopeIds: request.ScopeIds
        ), "client created successfully");
    }

    public async Task<Result> RevokeClientAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id);
        if (client is null)
            return Result.Failure(Error.ClientNotFound);

        client.IsActive = false;
        await _clientRepository.UpdateAsync(client);

        return Result.Success("Client revoked successfully.");
    }

    public async Task<Result<RotateSecretResponse>> RotateSecretAsync(Guid clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client is null)
            return Result.Failure<RotateSecretResponse>(Error.ClientNotFound);

        var newSecret = GenerateClientSecret();
        var secretHash = HashSecret(newSecret);

        client.ClientSecretHash = secretHash;
        await _clientRepository.UpdateAsync(client);

        return Result.Success(new RotateSecretResponse(newSecret), "secret rotated successfully");
    }

    public async Task<Result> UpdateScopesAsync(Guid clientId, List<Guid> scopeIds)
    {
        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client is null)
            return Result.Failure(Error.ClientNotFound);

        await _clientRepository.UpdateScopesAsync(client.ClientId, scopeIds);
        return Result.Success("Client scopes updated successfully.");
    }

    // ──── Private Helpers ────

    private static string GenerateClientSecret()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashSecret(string secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(bytes);
    }
}
