using ChatAdmin.Data;
using ChatAdmin.Hubs;
using ChatAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace ChatAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            List<chatUser> users = await _context.Users.OrderBy(p => p.Name).ToListAsync();
            return Ok(users);
        }
        [HttpGet("chats")]
        public async Task<IActionResult> GetChats(string senderId, string recipientId)
        {
            List<Message> chats =
                await _context.
                Chats.Where
                (p => p.SenderId == senderId && p.RecipientId == recipientId
                ||p.RecipientId==senderId&&p.SenderId==recipientId)
                .OrderBy(p=>p.SentAt)
                .ToListAsync();

            return Ok(chats);
        }
        [HttpGet("UserChatList")]
        public async Task<IActionResult> GetUsersWhoChattedWithAdmin()
        {
            // Find the admin user
            var admin = await _userManager.FindByNameAsync("admin@admin");
            if (admin == null)
            {
                return NotFound("Admin user not found.");
            }
            // Fetch user IDs that have interacted with the admin
            var userIds = await _context.Chats
                .Where(m => m.SenderId == admin.Id || m.RecipientId == admin.Id)
                .Select(m => m.SenderId == admin.Id ? m.RecipientId : m.SenderId)
                .Distinct()
                .ToListAsync();
            if (!userIds.Any())
            {
                return NotFound("No users have chatted with the admin.");
            }
            // Get users based on those IDs
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
            // Return the users as the response
            return Ok(users);
        }


    }
}



