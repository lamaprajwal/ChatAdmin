using ChatAdmin.Data;
using ChatAdmin.Hubs;
using ChatAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatAdmin.Controllers
{
    public class ChatsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<chatUser> _userManager;
       
        private readonly IHubContext<ChatHub> _hubContext;
        public ChatsController(AppDbContext context, UserManager<chatUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;

        }
        [HttpGet("RetriveUsers")]
        public async Task<IActionResult> GetUsers()
        {
            List<chatUser> users = await _context.Users.OrderBy(p => p.Name).ToListAsync();
            return Ok(users);
        }
        //[HttpPost("send-to-admin")]
        //public async Task<IActionResult> SendMessageToAdmin([FromBody] string messageContent)
        //{
        //    var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the sender ID from the authenticated user

        //    var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == "admin@admin");

        //    if (admin == null)
        //    {
        //        return NotFound("Admin user not found");
        //    }

        //    var message = new Message
        //    {
        //        SenderId = senderId,
        //        RecipientId = admin.Id,
        //        Content = messageContent,
        //        SentAt = DateTime.Now,
        //    };

        //    _context.Chats.Add(message);
        //    await _context.SaveChangesAsync();

        //    // Send message via SignalR to the admin
        //    await _hubContext.Clients.User(admin.Id).SendAsync("ReceiveMessage", senderId, messageContent);

        //    return Ok("Message sent to admin");
        //}

        //[HttpPost("send-to-user/{recipientId}")]
        //public async Task<IActionResult> SendMessageToUser(string recipientId, [FromBody] string messageContent)
        //{
        //    var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the admin ID from the authenticated user

        //    var message = new Message
        //    {
        //        SenderId = adminId,
        //        RecipientId = recipientId,
        //        Content = messageContent,
        //        SentAt = DateTime.Now,
        //    };

        //    _context.Chats.Add(message);
        //    await _context.SaveChangesAsync();

        //    // Send message via SignalR to the user
        //    await _hubContext.Clients.User(recipientId).SendAsync("ReceiveMessage", adminId, messageContent);

        //    return Ok("Message sent to user");
        //}
    }
}
