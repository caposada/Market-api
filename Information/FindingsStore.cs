using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextAnalysis;

namespace Information
{
    public class FindingsStore : StoreBase
    {
        public string? Text { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public ExchangeState StockExchangeState { get; set; }
        public List<AnalysisFinding>? Findings { get; set; }

        private string? folderName;

        public FindingsStore()
        {
        }

        public FindingsStore(string folderName)
        {
            this.folderName = folderName;
        }

        public override string GetFilename()
        {
            return "Findings";
        }

        public override string? GetFolderName()
        {
            return folderName;
        }

        public override string GetPathPrefix()
        {
            return Constants.GATHERER_FOLDER_NAME;
        }
    }
}
