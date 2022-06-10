using Elements;

namespace TextAnalysis
{
    public class AnalysisFinding
    {

        public BasicCompany Company { get; set; }
        public AnalysisConfidence Confidence { get; set; }
        public AnalysisRationale Rationale { get; set; }
        public List<AnalysisBreakDown.Token> Tokens { get; set; }

        public AnalysisFinding(
            BasicCompany company,
            AnalysisConfidence confidence,
            AnalysisRationale rationale,
            List<AnalysisBreakDown.Token> tokens)
        {
            this.Company = company;
            this.Confidence = confidence;
            this.Rationale = rationale;
            this.Tokens = tokens;
        }

        public override string ToString()
        {
            return Company.Name + " [" + Confidence.ToString() + "=" + Rationale.ToString() + "]";
        }

    }
}
