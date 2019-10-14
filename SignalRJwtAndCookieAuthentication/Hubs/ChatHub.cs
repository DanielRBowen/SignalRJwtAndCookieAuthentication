using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRJwtAndCookieAuthentication.Hubs
{
    //[Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var clientType = httpContext.Request.Query["ClientType"];
            HubClientStatus.ConnectedClients.Add((Context.UserIdentifier, clientType));
            await Clients.All.SendAsync("ReceiveSystemMessage", $"{Context.UserIdentifier} joined.");
            await SendConnectionStatus();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var httpContext = Context.GetHttpContext();
            var clientType = httpContext.Request.Query["ClientType"];
            HubClientStatus.ConnectedClients.RemoveWhere(connectedClient => connectedClient.userIdentifier == Context.UserIdentifier && connectedClient.clientType == clientType);
            await Clients.All.SendAsync("ReceiveSystemMessage", $"{Context.UserIdentifier} left.");
            await SendConnectionStatus();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendConnectionStatus()
        {
            try
            {
                //if (HubClientStatus.ConnectedClients.Any(connectedClient => connectedClient.userIdentifier == Context.UserIdentifier && connectedClient.clientType == "PC"))
                if (HubClientStatus.ConnectedClients.Any(connectedClient => connectedClient.clientType == "PC"))
                {
                    await Clients.All.SendAsync("sendConnectionStatus", true);
                    //await Client.User(Context.UserIdentifier).SendAsync("sendConnectionStatus", true);
                }
                else
                {
                    await Clients.All.SendAsync("sendConnectionStatus", false);
                    //await Clients.User(Context.UserIdentifier).SendAsync("sendConnectionStatus", false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public async Task SendToUser(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveDirectMessage", $"{Context.UserIdentifier}: {message}");
        }

        public async Task Send(string message)
        {
            if (Context.UserIdentifier == null)
            {
                return;
            }

            await Clients.All.SendAsync("ReceiveChatMessage", $"{Context.UserIdentifier}: {message}");
        }

        public async Task RequestLargeDataAsync()
        {
            if (Context.UserIdentifier == null)
            {
                return;
            }

            await Clients.User(Context.UserIdentifier).SendAsync("SendLargeDataAsync", Context.UserIdentifier);
        }
    }
}
