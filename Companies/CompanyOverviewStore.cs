using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companies
{
    public class CompanyOverviewStore : StoreBase
    {
        public List<CompanyOverview> Overviews { get; set; }

        public CompanyOverviewStore()
        {
            this.Overviews = new List<CompanyOverview>();
        }

        public override string GetFilename()
        {
            return "CompanyOverviewList";
        }

        public override string? GetFolderName()
        {
            return null;
        }

        public override string GetPathPrefix()
        {
            return Constants.COMPANIES_FOLDER_NAME;
        }
    }
}
