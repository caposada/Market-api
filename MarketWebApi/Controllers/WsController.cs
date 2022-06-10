using Elements;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MarketWebApi.Controllers
{
    public class WsController : ControllerBase, IDisposable
    {

        private WebSocket? webSocket;
        private readonly Market.App marketApp;

        public WsController(Market.App marketApp)
        {
            this.marketApp = marketApp;
            this.marketApp.EchoMessage += MarketApp_EchoMessage;
            this.marketApp.NewsManager.FreshArrivals += NewsManager_FreshArrivals;
            this.marketApp.NewsManager.SourceMonitorChanged += NewsManager_SourceMonitorChanged;
            this.marketApp.Gatherer.InterestedItemsChanged += Gatherer_InterestedItemsChanged;
            this.marketApp.CompanyDataStore.CompanyChanged += CompanyDataStore_CompanyChanged;
            this.marketApp.MarketRequestor.ResultReady += MarketRequestor_ResultReady;
            this.marketApp.MarketData.StateChange += MarketData_StateChange;
        }

        public void Dispose()
        {
            this.marketApp.EchoMessage -= MarketApp_EchoMessage;
            this.marketApp.NewsManager.FreshArrivals -= NewsManager_FreshArrivals;
            this.marketApp.NewsManager.SourceMonitorChanged -= NewsManager_SourceMonitorChanged;
            this.marketApp.Gatherer.InterestedItemsChanged -= Gatherer_InterestedItemsChanged;
            this.marketApp.CompanyDataStore.CompanyChanged -= CompanyDataStore_CompanyChanged;
            this.marketApp.MarketRequestor.ResultReady -= MarketRequestor_ResultReady;
            this.marketApp.MarketData.StateChange -= MarketData_StateChange;
        }

        [HttpGet("/websocket")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async void MarketApp_EchoMessage(object obj)
        {
            ColourConsole.WriteLine(DateTime.Now + " : MarketApp_EchoMessage", ConsoleColor.White, ConsoleColor.DarkGray);
            string jsonString = JsonSerializer.Serialize((WebSocketMessage)obj);
            await Send(jsonString);
        }

        private async void NewsManager_FreshArrivals(Guid id, List<Elements.NewsItem> freshItems)
        {
            string title = marketApp.NewsManager.GetSource(id).SourceMonitor.Feed.Title;
            string jsonString = JsonSerializer.Serialize(new WebSocketMessage("NewsManager", "FreshArrivals")
            {
                Id = id.ToString(),
                Name = title,
                Data = freshItems.Count.ToString()
            });
            await Send(jsonString);
        }

        private async void NewsManager_SourceMonitorChanged(Guid id, string eventName)
        {
            string title = marketApp.NewsManager.GetSource(id).SourceMonitor.Feed.Title;
            string jsonString = JsonSerializer.Serialize(new WebSocketMessage("NewsManager", "SourceMonitorChanged")
            {
                Id = id.ToString(),
                Name = eventName
            });
            await Send(jsonString);
        }

        private async void Gatherer_InterestedItemsChanged(int numberOfInterestingItems)
        {
            ColourConsole.WriteLine(DateTime.Now + " : Gatherer_InterestedItemsAdded", ConsoleColor.White, ConsoleColor.DarkGray);
            string jsonString = JsonSerializer.Serialize(new WebSocketMessage("Gatherer", "InterestedItemsChanged")
            {
                Data = numberOfInterestingItems.ToString()
            });
            await Send(jsonString);
        }

        private async void CompanyDataStore_CompanyChanged(string symbol)
        {
            string jsonString = JsonSerializer.Serialize(new WebSocketMessage("Company", "CompanyChanged")
            {
                Id = symbol
            });
            await Send(jsonString);
        }

        private async void MarketRequestor_ResultReady(Guid id, string name)
        {
            string jsonString = JsonSerializer.Serialize(new WebSocketMessage("Market", "ResultReady")
            {
                Id = id.ToString(),
                Name = name
            });
            await Send(jsonString);
        }

        private async void MarketData_StateChange()
        {
            string jsonString = JsonSerializer.Serialize(new WebSocketMessage("Market", "StateChange")
            {
                Data = marketApp.MarketData.Status.ToString()
            });
            await Send(jsonString);
        }

        private async Task Send(string message)
        {
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }

    }
}
