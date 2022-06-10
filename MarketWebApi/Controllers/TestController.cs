using Microsoft.AspNetCore.Mvc;

namespace MarketWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private readonly Market.App marketApp;

        public TestController(Market.App marketApp)
        {
            this.marketApp = marketApp;
        }

        [HttpPost()]
        public void WebsocketEcho([FromBody] WebSocketMessage webSocketMessage)
        {
            marketApp.WebsocketEcho(webSocketMessage);
        }
    }
}
