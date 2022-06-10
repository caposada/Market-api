using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Information
{
    public class TimeSeriesesStore : StoreBase
    {
        public List<StockTimeSeriesResult>? StockTimeSeriesResults { get; set; }

        private string folderName;

        public TimeSeriesesStore()
        {
        }

        public TimeSeriesesStore(string folderName)
        {
            this.folderName = folderName;
        }

        public override string GetFilename()
        {
            return "TimeSerieses";
        }

        public override string GetFolderName()
        {
            return folderName;
        }

        public override string GetPathPrefix()
        {
            return Constants.GATHERER_FOLDER_NAME;
        }
    }
}
