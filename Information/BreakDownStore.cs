using DataStorage;
using Elements;
using TextAnalysis;

namespace Information
{
    public class BreakDownStore : StoreBase
    {

        public AnalysisBreakDown? AnalysisBreakDown { get; set; }

        private string? folderName;

        public BreakDownStore()
        {
        }

        public BreakDownStore(string folderName)
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
}
