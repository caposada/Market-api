using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Information
{
    public class GathererInformationStore : StoreBase
    {
        public List<InterestingItem> InterestingItems { get; set; }
        public List<NonInterestingItem> NonInterestingItems { get; set; }
        public DateTime LatestDate { get; set; }

        public GathererInformationStore()
        {
            this.InterestingItems = new List<InterestingItem>();
            this.NonInterestingItems = new List<NonInterestingItem>();
            this.LatestDate = DateTime.MinValue;
        }

        public override string GetFilename()
        {
            return "Information";
        }

        public override string? GetFolderName()
        {
            return null;
        }

        public override string GetPathPrefix()
        {
            return Constants.GATHERER_FOLDER_NAME;
        }
    }
}
