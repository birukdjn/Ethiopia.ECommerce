using Ethiopia.Domain.Enums;

namespace Ethiopia.Domain.Entities
{
    public class ApplicationUser
    {
        public string FullName { get; set; }= string.Empty;
        public UserType UserType { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;

        public virtual VendorProfile? VendorProfile { get; set; }
    }
}
