using Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companies
{
    public class CompaniesAliasListStore : StoreBase
    {
        public List<CompanyAlias> Aliases { get; set; }

        public CompaniesAliasListStore()
        {
            this.Aliases = new List<CompanyAlias>();
        }

        public override string GetFilename()
        {
            return "CompaniesAliasList";
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
