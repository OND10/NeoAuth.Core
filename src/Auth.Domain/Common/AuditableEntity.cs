namespace Auth.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Provides a consistent primary key, creation timestamp, and optional soft-delete
/// and update tracking out of the box.
///
/// Usage guidelines (senior .NET perspective):
///  - Every entity that is persisted to the DB should inherit this.
///  - UpdatedAt is set by EF Core interceptors / SaveChanges override — not manually in business code.
///  - IsDeleted enables soft-deletes; use a global query filter in DbContext so deleted
///    rows are never accidentally returned.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>Primary key — always a new GUID on construction.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the record was first inserted.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the last update. Null until first modification.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag.
    /// Filtered out automatically by the DbContext global query filter.
    /// </summary>
    public bool IsDeleted { get; set; }
}
