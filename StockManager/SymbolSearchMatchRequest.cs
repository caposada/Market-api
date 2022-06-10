namespace StockManager
{
    public class SymbolSearchMatchRequest
    {
        public string KeyWords { get; set; }

        public SymbolSearchMatchRequest(string keyWords)
        {
            KeyWords = keyWords;
        }
    }
}
