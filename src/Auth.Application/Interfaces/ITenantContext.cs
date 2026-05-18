using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Application.Interfaces
{
    /// <summary>
    /// Provides the current tenant context (set by middleware).
    /// </summary>
    public interface ITenantContext
    {
        Guid? TenantId { get; }
    }
}
