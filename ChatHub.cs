using Microsoft.AspNetCore.SignalR;

namespace dotnet_primer;
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", "System", "echo: "+message);
    }
}