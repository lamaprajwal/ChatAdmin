using ChatAdmin.Data;
using ChatAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAdmin.Controllers
{
    public class ChatsController : Controller
    {
        private readonly AppDbContext _context;
        public ChatsController(AppDbContext context)
        {
            _context = context;
            
        }
        [HttpGet("RetriveUsers")]
        public async Task<IActionResult> GetUsers()
        {
            List<chatUser> users = await _context.Users.OrderBy(p => p.Name).ToListAsync();
            return Ok(users);
        }
    }
}
