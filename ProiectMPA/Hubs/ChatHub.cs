using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProiectMPA.Models;

namespace ProiectMPA.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            string username = Context.User?.Identity?.Name ?? "Anonymous";
            Console.WriteLine(message);
            await Clients.All.SendAsync("ReceiveMessage", username, message);
        }
    }
}