using Auth.Domain.Enums;

namespace Auth.Application.DTOs;

// ──────────────────────────── Device DTOs ────────────────────────────

/// <summary>Response DTO for a registered device.</summary>
public record DeviceResponse(
    Guid Id,
    string FriendlyName,
    string? Platform,
    string? DeviceType,
    string? IpAddress,
    DeviceStatus Status,
    DateTime LastActivityAt,
    DateTime CreatedAt
);

/// <summary>Sent by the client on login / refresh to identify the device.</summary>
public record DeviceInfo(
    string DeviceFingerprint,
    string? FriendlyName = null,
    string? Platform = null,
    string? DeviceType = null
);

/// <summary>Rename a device.</summary>
public record RenameDeviceRequest(string FriendlyName);
