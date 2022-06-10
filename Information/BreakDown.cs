using Elements;
using TextAnalysis;

namespace Information
{
    public class BreakDown
    {
        public class Store : StoreBase
        {

            public AnalysisBreakDown? AnalysisBreakDown { get; set; }

            private string? folderName;

            public Store()
            {
            }

            public Store(string folderName)
            {
                this.folderName = folderName;
            }

            public override string GetFilename()
            {
                return "BreakDown";
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

        public Guid Id { get; set; }
        public AnalysisBreakDown? AnalysisBreakDown
        {
            get
            {
                if (store.Data.AnalysisBreakDown == null)
                    store.Load();
                return store.Data.AnalysisBreakDown;
            }
            set
            {
                store.Data.AnalysisBreakDown = value;
                store.Save();
            }
        }

        private DataStorage<Store> store;

        public BreakDown(Guid id)
        {
            this.Id = id;
            store = new DataStorage<Store>(new Store(this.Id.ToString()));
        }

    }
}
