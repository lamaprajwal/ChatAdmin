using Microsoft.AspNetCore.Identity;

namespace ChatAdmin.Models
{
    public class chatUser:IdentityUser
    {
        public string Name {  get; set; }
        public string Status {  get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> ReceivedMessages { get; set; }
    }
}
