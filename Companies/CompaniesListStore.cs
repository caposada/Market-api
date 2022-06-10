using DataStorage;
using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companies
{
    public class CompaniesListStore : StoreBase
    {
        public List<SimpleCompany>? Companies { get; set; }

        public CompaniesListStore()
        {
            this.Companies = new List<SimpleCompany>();
        }

        public override string GetFilename()
        {
            return "CompaniesList";
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
