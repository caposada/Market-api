using DataStorage;
using Elements;
using System.Text.Json.Serialization;

namespace Companies
{
    public class CompaniesAliasList : IDataStoragable<CompaniesAliasListStore>
    {        

        public List<CompanyAlias> Aliases
        {
            get
            {
                return Store.Data.Aliases;
            }
        }

        [JsonIgnore]
        public DataStorage<CompaniesAliasListStore>? Store { get; set; }

        public CompaniesAliasList()
        {
            this.Store = new DataStorage<CompaniesAliasListStore>(new CompaniesAliasListStore());
            this.Store.Load();
        }

        public CompanyAlias SetAlias(string symbol, List<string> aliases)
        {
            CompanyAlias? alias = Store.Data.Aliases.Find(x => x.Symbol == symbol);
            if (alias != null)
            {
                alias.Names = aliases;
                this.Store.Save();
            }
            else
            {
                alias = new CompanyAlias(symbol, aliases);
                this.Store.Data.Aliases.Add(alias);
                this.Store.Save();
            }
            return alias;
        }

        public List<string> GetAllAliases()
        {
            List<string> aliases = new List<string>();
            foreach (var alias in Store.Data.Aliases)
            {
                aliases.AddRange(alias.Names);
            }
            return aliases;
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
