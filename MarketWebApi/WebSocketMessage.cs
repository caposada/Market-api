namespace MarketWebApi
{
    public class WebSocketMessage
    {
        public string Root { get; set; }
        public string EventName { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Data { get; set; }

        public WebSocketMessage(string root, string eventname)
        {
            this.Root = root;
            this.EventName = eventname;
        }

    }
}
