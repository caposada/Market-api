using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elements;
using TwitterFeedReader;

namespace TextAnalysis.Tests
{
    [TestClass()]
    public class AnalysisInfoTests
    {
        static TwitterFeed twitterFeed = new TwitterFeed(Guid.NewGuid(), "", "");
        static NewsItem newsItem = new NewsItem(
                "123",
                DateTimeOffset.Now,
                "Weary from the pandemic and pressured by inflation, Apple store employees are holding votes on whether to unionize.",
                twitterFeed);
        static BasicCompany company1 = new BasicCompany()
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Name = "Apple",
            Exchange = Exchanges.NYSE
        };
        static BasicCompany company2 = new BasicCompany()
        {
            Id = Guid.NewGuid(),
            Symbol = "FB",
            Name = "Facebook",
            Exchange = Exchanges.NYSE
        };

        [TestMethod()]
        public void AnalysisInfoTest()
        {
            AnalysisInfo analysisInfo = new AnalysisInfo(newsItem);

            Assert.AreEqual(newsItem, analysisInfo.NewsItem, "Should be equal.");
            Assert.AreEqual(newsItem.Text, analysisInfo.Text, "Should be equal.");
            Assert.AreEqual(0, analysisInfo.Findings.Count, "Should be zero.");
            Assert.IsNull(analysisInfo.BreakDown, "Should be null.");
            Assert.IsFalse(analysisInfo.Interesting, "Should be false.");
        }

        [TestMethod()]
        public void SetAnalysisBreakDownTest()
        {
            AnalysisInfo analysisInfo = new AnalysisInfo(newsItem);

            Assert.IsNull(analysisInfo.BreakDown, "Should be null.");

            AnalysisBreakDown analysisBreakDown = new AnalysisBreakDown(null);
            analysisInfo.SetAnalysisBreakDown(analysisBreakDown);

            Assert.AreEqual(analysisBreakDown, analysisInfo.BreakDown, "Should be equal.");
        }

        [TestMethod()]
        public void AddFindingTest()
        {
            AnalysisInfo analysisInfo = new AnalysisInfo(newsItem);

            Assert.AreEqual(0, analysisInfo.Findings.Count, "Should initially be no findings.");

            analysisInfo.AddFinding(new AnalysisFinding(company1, AnalysisConfidence.HIGH, AnalysisRationale.FULL_NAME, null));
            Assert.AreEqual(1, analysisInfo.Findings.Count, "Should now have 1 finding.");

            analysisInfo.AddFinding(new AnalysisFinding(company1, AnalysisConfidence.HIGH, AnalysisRationale.FULL_NAME, null));
            Assert.AreEqual(1, analysisInfo.Findings.Count, "Should still be have 1 finding, because same company added.");

            analysisInfo.AddFinding(new AnalysisFinding(company2, AnalysisConfidence.HIGH, AnalysisRationale.FULL_NAME, null));
            Assert.AreEqual(2, analysisInfo.Findings.Count, "Should now be have 2 finding.");
        }

        [TestMethod()]
        public void AddCompanyFindingTest()
        {
            AnalysisInfo analysisInfo = new AnalysisInfo(newsItem);

            Assert.AreEqual(0, analysisInfo.Findings.Count, "Should initially be no findings.");

            analysisInfo.AddCompanyFinding(company1, AnalysisConfidence.HIGH, AnalysisRationale.FULL_NAME, null);
            Assert.AreEqual(1, analysisInfo.Findings.Count, "Should now have 1 finding.");

            analysisInfo.AddCompanyFinding(company1, AnalysisConfidence.HIGH, AnalysisRationale.FULL_NAME, null);
            Assert.AreEqual(1, analysisInfo.Findings.Count, "Should still be have 1 finding, because same company added.");

            analysisInfo.AddCompanyFinding(company2, AnalysisConfidence.HIGH, AnalysisRationale.FULL_NAME, null);
            Assert.AreEqual(2, analysisInfo.Findings.Count, "Should now be have 2 finding.");
        }
    }
}