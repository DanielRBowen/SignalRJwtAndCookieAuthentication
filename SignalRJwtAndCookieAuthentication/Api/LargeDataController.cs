using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRJwtAndCookieAuthentication.Dtos;
using SignalRJwtAndCookieAuthentication.Hubs;
using System.Threading.Tasks;

namespace SignalRJwtAndCookieAuthentication.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class LargeDataController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public LargeDataController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("submitlargedata")]
        public async Task SubmitLargeDataAsync([FromBody]SubmitLargeDataCommand submitLargeDataCommand)
        {
            await _hubContext.Clients.User(submitLargeDataCommand.User).SendAsync("UpdateLargeData", submitLargeDataCommand.JsonData);
        }
    }
}
