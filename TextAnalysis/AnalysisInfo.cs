using Elements;
using System.Text.RegularExpressions;

namespace TextAnalysis
{
    public class AnalysisInfo
    {
        private List<AnalysisFinding> findings;
        private static Regex removeWordsRegex = new Regex(
            @"(LISTEN NOW:|LIVE NOW ON YOUTUBE!|CLICK TO WATCH|Q1|Q2|Q3|Q4|PM|UK)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public NewsItem NewsItem { get; private set; }
        public bool Interesting
        {
            get
            {
                // If content mentions a company (or other interesting information)
                // then it should be deemed as interesting.
                return Findings.Count > 0;
            }
        }
        public AnalysisConfidence HighestConfidence
        {
            get
            {
                return findings.Max(x => x.Confidence);
            }
        }
        public AnalysisConfidence LowestConfidence
        {
            get
            {
                return findings.Min(x => x.Confidence);
            }
        }
        public AnalysisBreakDown? BreakDown { get; private set; }
        public List<AnalysisFinding> Findings
        {
            get
            {
                return findings.OrderByDescending(x => (int)(x.Confidence)).ToList();
            }
        }
        public string Text { get; private set; }

        public AnalysisInfo(NewsItem newsItem)
        {
            this.NewsItem = newsItem;
            this.Text = RemoveUnwantedGarbage(this.NewsItem.Text);
            this.findings = new List<AnalysisFinding>();
        }

        public void SetAnalysisBreakDown(AnalysisBreakDown analysisBreakDown)
        {
            this.BreakDown = analysisBreakDown;
        }

        public void AddFinding(AnalysisFinding finding)
        {
            // Check we haven't already added this company
            if (finding.Company != null && findings.Find(x => x.Company == finding.Company) == null)
            {
                findings.Add(finding);
            }
        }

        public void AddCompanyFinding(
            SimpleCompany company,
            AnalysisConfidence confidence,
            AnalysisRationale rationale,
            List<AnalysisBreakDown.Token> tokens)
        {
            AnalysisFinding finding = new AnalysisFinding(
                company,
                confidence,
                rationale,
                tokens);
            AddFinding(finding);
        }

        private static string RemoveUnwantedGarbage(string text)
        {
            // Now remove whole words like:
            // The, Limited, Ltd, Group, Inc
            text = removeWordsRegex.Replace(text, "");

            text = text.Trim();

            return text;
        }
    }
}
