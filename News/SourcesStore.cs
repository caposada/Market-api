using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News
{
    public class SourcesStore : StoreBase
    {

        public List<Source> Sources { get; set; }

        public SourcesStore()
        {
            this.Sources = new List<Source>();
        }

        public override string GetFilename()
        {
            return "Sources";
        }

        public override string? GetFolderName()
        {
            return null;
        }

        public override string GetPathPrefix()
        {
            return Constants.FEED_FOLDER_NAME;
        }
    }
}
