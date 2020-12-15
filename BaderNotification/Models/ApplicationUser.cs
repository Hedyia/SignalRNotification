using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace BaderNotification.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Notification> Notifications { get; set; }
    }
}
