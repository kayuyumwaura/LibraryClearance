using Microsoft.AspNetCore.Identity;

namespace LibraryClearance.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public ICollection<ClearanceRequest> Requests { get; set; } = new List<ClearanceRequest>();
    }
}