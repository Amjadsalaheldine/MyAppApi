using Microsoft.AspNetCore.Identity;

namespace MyAppApi.Models
{
    public class ApplicationUser : IdentityUser
    {
      
        public string FullName { get; set; }
    }
}