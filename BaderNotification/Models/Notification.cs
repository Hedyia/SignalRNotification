using System;

namespace BaderNotification.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
        public bool Seen { get; set; }
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }
}
