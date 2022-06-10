using Elements;

namespace Companies
{
    public class CompaniesAliasList
    {

        public class Store : StoreBase
        {
            public List<CompanyAlias> Aliases { get; set; }

            public Store()
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

        public List<CompanyAlias> Aliases
        {
            get
            {
                return store.Data.Aliases;
            }
        }

        private DataStorage<Store> store;

        public CompaniesAliasList()
        {
            this.store = new DataStorage<Store>(new Store());
            this.store.Load();
        }

        public CompanyAlias SetAlias(string symbol, List<string> aliases)
        {
            CompanyAlias? alias = store.Data.Aliases.Find(x => x.Symbol == symbol);
            if (alias != null)
            {
                alias.Names = aliases;
                this.store.Save();
            }
            else
            {
                alias = new CompanyAlias(symbol, aliases);
                this.store.Data.Aliases.Add(alias);
                this.store.Save();
            }
            return alias;
        }

        public List<string> GetAllAliases()
        {
            List<string> aliases = new List<string>();
            foreach (var alias in store.Data.Aliases)
            {
                aliases.AddRange(alias.Names);
            }
            return aliases;
        }

    }
}
