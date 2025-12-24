using System;
using System.Collections.Generic;
using System.Text;

namespace Ethiopia.Domain.Entities
{
    public class VendorProfile
    {
        public Guid Id { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
