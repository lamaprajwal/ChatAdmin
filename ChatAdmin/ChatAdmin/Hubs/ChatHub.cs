using ChatAdmin.Data;
using ChatAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System;

namespace ChatAdmin.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext context;
        private readonly UserManager<chatUser> _userManager;
        public ChatHub(AppDbContext context, UserManager<chatUser> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }


        public static Dictionary<string, Guid> Users = new();
        public async Task Connect(Guid userId)
        {
            Users.Add(Context.ConnectionId, userId);
            chatUser user = await context.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = "online";
                await context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid userId;
            Users.TryGetValue(Context.ConnectionId, out userId);
            Users.Remove(Context.ConnectionId);
            chatUser user = await context.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = "offline";
                await context.SaveChangesAsync();

                await Clients.All.SendAsync("Users", user);
            }
        }
        public async Task SendMessageToAdmin(string messageContent)
        {
            var senderId = Context.UserIdentifier;
            var admin = await _userManager.Users.FirstOrDefaultAsync(u => u.Name=="admin@admin");

            var message = new Message
            {
                SenderId = senderId,
                RecipientId = admin.Id,
                Content = messageContent,
                SentAt = DateTime.Now,
            };

            context.Chats.Add(message);
            await context.SaveChangesAsync();

            await Clients.User(admin.Id).SendAsync("ReceiveMessage", senderId, messageContent);
        }
        public async Task SendMessageToUser(string recipientId, string messageContent)
        {
            var adminId = Context.UserIdentifier;

            var message = new Message
            {
                SenderId = adminId,
                RecipientId = recipientId,
                Content = messageContent,
                SentAt = DateTime.Now,
            };

            context.Chats.Add(message);
            await context.SaveChangesAsync();

            await Clients.User(recipientId).SendAsync("ReceiveMessage", adminId, messageContent);
        }
    }
}
