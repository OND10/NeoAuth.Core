using Auth.Application.DTOs;
using Auth.Domain.Common;

namespace Auth.Application.Interfaces;

public interface IClientAuthService
{
    Task<Result<ClientCredentialsResponse>> AuthenticateAsync(ClientCredentialsRequest request);
    Task<Result<CreateClientResponse>> CreateClientAsync(CreateClientRequest request);
    Task<Result> RevokeClientAsync(Guid id);
    Task<Result<RotateSecretResponse>> RotateSecretAsync(Guid clientId);
    Task<Result> UpdateScopesAsync(Guid clientId, List<Guid> scopeIds);
}
