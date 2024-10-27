using ChatAdmin.Data;
using ChatAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using Azure.Messaging;

namespace ChatAdmin.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext context;
        private readonly UserManager<chatUser> _userManager;
        private static readonly ConcurrentDictionary<string, string> Users = new();
        private static readonly List<string> chattedUsers = new();
        public ChatHub(AppDbContext context, UserManager<chatUser> userManager)
        {
            this.context = context;
            _userManager = userManager;
        }
        public async Task Connect(string userId)
        {
            Users.TryAdd(Context.ConnectionId, userId);
            // Load user and update status
            var user = await context.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Status = "online";
                await context.SaveChangesAsync();
                await Clients.All.SendAsync("Users", user);
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Users.TryRemove(Context.ConnectionId, out string userId))
            {
                var user = await context.Users.FindAsync(userId);
                if (user is not null)
                {
                    user.Status = "offline";
                    await context.SaveChangesAsync();
                    await Clients.All.SendAsync("Users", user);
                }
            }
        }
        public async Task SendMessageToAdmin(string messageContent, string senderId)
        {
            // Fetch the admin user
            chatUser admin = await _userManager.Users.FirstOrDefaultAsync(u => u.Name == "admin@admin");
            if (admin == null) throw new Exception("Admin not found.");
            // Create and store the message in the database
            var message = new Message
            {
                SenderId = senderId,
                RecipientId = admin.Id,
                Content = messageContent,
                SentAt = DateTime.Now
            };
            context.Chats.Add(message);
            await context.SaveChangesAsync();
            // Check if the admin is connected
            var connection = Users.FirstOrDefault(p => p.Value == admin.Id);
            if(!chattedUsers.Contains(senderId))
            {
                chattedUsers.Add(senderId);
            }

            if (connection.Key != null)
            {
                // Send the message in real-time if the admin is online
                await Clients.Client(connection.Key).SendAsync("ReceiveMessage", senderId, messageContent);
            }
            // Send the message back to the sender for confirmation
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, messageContent);
        }
        public async Task SendMessageToUser(string recipientId, string messageContent)
        {
            // Fetch the admin user
            chatUser admin = await _userManager.Users.FirstOrDefaultAsync(u => u.Name == "admin@admin");
            if (admin == null) throw new Exception("Admin not found.");
            // Create and store the message in the database
            var message = new Message
            {
                SenderId = admin.Id,
                RecipientId = recipientId,
                Content = messageContent,
                SentAt = DateTime.Now,
            };
            context.Chats.Add(message);
            await context.SaveChangesAsync();
            if (!chattedUsers.Contains(recipientId))
            {
                chattedUsers.Add(recipientId);
            }
            // Check if the recipient is connected and send the message in real-time
            if (Users.Any(u => u.Value == recipientId))
            {
                string connectionId = Users.First(p => p.Value == recipientId).Key;
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", admin.Id, messageContent);
            }
            // Send the message back to the admin
            await Clients.Caller.SendAsync("ReceiveMessage", admin.Id, messageContent);
        }
        
            public Task<List<string>> GetUsersWhoChattedWithAdmin()
            {
                return Task.FromResult(chattedUsers);
            }
        public async Task GetIdAllId()
        {
            var admin = await _userManager.FindByNameAsync("admin@admin");
            
            // Fetch user IDs that have interacted with the admin
            var userIds = await context.Chats
                .Where(m => m.SenderId == admin.Id || m.RecipientId == admin.Id)
                .Select(m => m.SenderId == admin.Id ? m.RecipientId : m.SenderId)
                .Distinct()
                .ToListAsync();
            var connection = Users.FirstOrDefault(p => p.Value == admin.Id);

            await Clients.Client(connection.Key).SendAsync("ReceiveMessage", userIds);
        }
    }

}



