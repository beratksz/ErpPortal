using System.Collections.Generic;

namespace ErpPortal.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Should be hashed
        public string FullName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; } = true;
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;
        public System.DateTime? LastLoginAt { get; set; }
        public ICollection<UserWorkCenter> UserWorkCenters { get; set; } = new List<UserWorkCenter>();
    }
}
