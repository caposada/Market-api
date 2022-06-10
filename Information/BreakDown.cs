using Elements;
using System.Text.Json.Serialization;
using TextAnalysis;

namespace Information
{

    public class BreakDown : IDataStoragable<BreakDownStore>
    {        

        public Guid Id { get; set; }
        public AnalysisBreakDown? AnalysisBreakDown
        {
            get
            {
                if (Store.Data.AnalysisBreakDown == null)
                    Store.Load();
                return Store.Data.AnalysisBreakDown;
            }
            set
            {
                Store.Data.AnalysisBreakDown = value;
                Store.Save();
            }
        }

        [JsonIgnore]
        public DataStorage<BreakDownStore>? Store { get; set; }

        public BreakDown(Guid id)
        {
            this.Id = id;
            Store = new DataStorage<BreakDownStore>(new BreakDownStore(this.Id.ToString()));
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }
    }
}
