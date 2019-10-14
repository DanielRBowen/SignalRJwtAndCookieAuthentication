using System.Collections.Generic;

namespace SignalRJwtAndCookieAuthentication.Hubs
{
    public class HubClientStatus
    {
        public static HashSet<(string userIdentifier, string clientType)> ConnectedClients = new HashSet<(string, string)>();
    }
}
