using System;

namespace ChatAdmin.Models
{
    public class Message
    {
        public Message()
        {
            
                Id = Guid.NewGuid();
        }
           
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public string SenderId { get; set; }
        public chatUser Sender { get; set; }

        public string RecipientId { get; set; }
        public chatUser Recipient { get; set; }
    }
}
