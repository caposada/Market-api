using Catalyst;
using Companies;
using Elements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockManager;
using TwitterFeedReader;

namespace TextAnalysis.Tests
{
    [TestClass()]
    public class AnalyserTests
    {
        static CompanyDataStore companyDataStore;
        static Analyser analyser;
        static string text_Apple = "Apple do well";
        static string text_Google = "Upbeat news for Google this summer";
        static string text_Toyota_withoutDollar = "Toyota TM said today it missed its global production target";
        static string text_Apple_withDollar = "The news is $AAPL do well";
        static string text_Apple_withoutDollar = "The news is AAPL do well";
        static TwitterFeed twitterFeed = new TwitterFeed(Guid.NewGuid(), "", "");

        // Bad (but found)
        // 'UK' <= UK says Russia is using separatist forces in eastern Ukraine; explosions reported in Kyiv https://t.co/PQzno48W3a


        [ClassInitialize]
        public static void AnalyserTestsSetup(TestContext context)
        {
            // Executes once for the test class. (Optional)
            MarketData marketData = new MarketData();
            companyDataStore = new CompanyDataStore(marketData);
            analyser = new Analyser(companyDataStore);
        }

        [TestMethod()]
        public void AnalyseTest()
        {
            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "Weary from the pandemic and pressured by inflation, Apple store employees are holding votes on whether to unionize.",
                twitterFeed));
            Assert.AreEqual(
                1,
                analyser.Info.Findings.Count,
                "Should have 1 (Apple) findings");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "A married pair of scientists pleaded guilty to taking confidential data from a Pfizer lab where she worked and sending it to China. " +
                "The plot unraveled after the husband was caught trying to sneak toxic chemicals into the U.S.from China to start a lab.",
                twitterFeed));
            Assert.AreEqual(
                2,
                analyser.Info.Findings.Count,
                "Should have 2 findings - one HIGH (Pfizer) and 1 MEDIUM (Aluminum Corporation Of China - because of 'China') confidence");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "Nvidia's 'transformation' is 'underappreciated,' BofA analysts say",
                twitterFeed));
            Assert.AreEqual(
                1,
                analyser.Info.Findings.Count,
                "Should have 1 (NVIDIA) finding");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "SWVL plans to lay off 32% of its team two months after going public",
                twitterFeed));
            Assert.AreEqual(
                1,
                analyser.Info.Findings.Count,
                "Should have 1 (Swvl) finding - SWVL at the begining a is a symbol");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "GoTo Growth Tops 50% in First Report Since $1.1 Billion IPO",
                twitterFeed));
            Assert.AreEqual(
                1,
                analyser.Info.Findings.Count,
                "Should have 1 (First Trust Active Factor Large Cap ETF - because of 'First') finding - with a Medium confidence");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "Best performing large cap stoocks in today's early trading Pinduodouo $PDD $APA Smucker $SJM Exxon $XOM BeiiGene $BGNE CrowdStrike $CRWD Oviintiv $OVV Take-Two $TTWO $BHP Continental $CLR Texas Pacific $TPL John Hancock $BTO Valeroo $VLO Taarga $TRGP Rio Tinto $RIO Conoco $COP",
                twitterFeed));
            Assert.AreEqual(
                16,
                analyser.Info.Findings.Count,
                "Should have 16 finding - all those with $[A-Z]+ are company symbols");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "How the largest stocks performed today Apple $AAPL +0.5% Microsoft $MSFT -0.5% Google $GOOGL +2% Amazon $AMZN +2% Tesla $TSLA +1.6% Berkshire $BRK.B +0.4% Facebook $FB +1.8% Nvidia $NVDA +0.4% TSMC $TSM -0.7% $JNJ -0.01% UnitedHealth $UNH +0.9% Visa $V +0.1% Exxon $XOM -0.3%",
                twitterFeed));
            Assert.AreEqual(
                13,
                analyser.Info.Findings.Count,
                "Should have 13 finding - all those with $[A-Z]+ are company symbols"); // 13 +/- 1???

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "LISTEN NOW: British PM Boris Johnson survives a “no confidence” vote, the saga between Elon Musk and Twitter continues and more. Listen and follow the @CNBCWEX podcast here or on your favorite podcast platform: https://t.co/i2F0hap7uz https://t.co/sblVAHYljt",
                twitterFeed));
            Assert.AreEqual(
                2,
                analyser.Info.Findings.Count,
                "Should have 2 finding - one HIGH (Twitter) and one MEDIUM (British American Tobacco because of 'British') and not DOW (NOW) - 'LISTEN NOW:' is garbage");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "LISTEN NOW: The Dow closed up over 200 points and Jim Cramer is in San Francisco breaking down all the headline that moved the averages today. Listen and follow the @MadMoneyOnCNBC podcast here or on your favorite podcast platform: https://t.co/SB7Pdr5o6g https://t.co/1WS46EwXwA",
                twitterFeed));
            Assert.AreEqual(
                1,
                analyser.Info.Findings.Count,
                "Should have 1 finding - one HIGH (Dow) and not DOW (NOW) - 'LISTEN NOW:' is garbage");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "$TIRX - Tian Ruixiang gets Nasdaq notification regarding minimum bid price deficiency https://t.co/5vj9Q4rfuM",
                twitterFeed));
            Assert.AreEqual(
                2,
                analyser.Info.Findings.Count,
                "Should have 2 finding - one HIGH (Tian Ruixiang(TIRX)) and one MEDIUM (Nasdaq because 'Nasdaq')");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "$AAPL $AFRM $KLAR - Is a bank charter in the cards after Apple touts 'Pay Later' program? https://t.co/HQaVLM6Et0",
                twitterFeed));
            Assert.AreEqual(
                2,
                analyser.Info.Findings.Count,
                "Should have 2 finding - KlaraBo Sverige AB (KLAR) not in NYSE or NASDAQ markets");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "$JNPR - XL Axiata team up with Juniper Networks to accelerate the rollout of 5G services across Indonesia https://t.co/bBwAITwl3e",
                twitterFeed));
            Assert.AreEqual(
                2,
                analyser.Info.Findings.Count,
                "Should have 2 finding - one HIGH (with two markers - 'JNPR' and 'Juniper Networks') and one MEDIUM (iShares MSCI Indonesia ETF - which is based on 'Indonesia')");

            _ = analyser.Analyse(new NewsItem(
                "123",
                DateTimeOffset.Now,
                "Nestle CEO does not see 'significant' baby formula shortages outside U.S.",
                twitterFeed));
            Assert.AreEqual(
                0,
                analyser.Info.Findings.Count,
                "Should have 0 finding - Nestle not in NYSE or NASDAQ markets");




        }

        [TestMethod()]
        public void CheckForSymbolsTest()
        {
            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple_withDollar, twitterFeed));
            Assert.AreEqual(1, analyser.Info.Findings.Count, "Should have one 'finding' for the company 'Apple' based on Twitter $symbol convention");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Toyota_withoutDollar, twitterFeed));
            Assert.AreEqual(1, analyser.Info.Findings.Count, "Should have one 'finding' for the company 'Toyata' as name and symbol in text");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple_withoutDollar, twitterFeed));
            Assert.AreEqual(0, analyser.Info.Findings.Count, "Should have ZERO 'finding' for the company 'Apple' as only symbol without dollar prefix");
        }

        [TestMethod()]
        public async Task HasFragmentTest()
        {
            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple, twitterFeed));
            Assert.AreEqual(
                "Apple",
                analyser.HasFragment("Apple", 0, PartOfSpeech.PROPN, await companyDataStore.GetCompaniesFromFragment("Apple")).Name,
                "Should find 'Apple' company");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple, twitterFeed));
            Assert.AreEqual(
                "Alphabet",
                analyser.HasFragment("Google", 0, PartOfSpeech.PROPN, await companyDataStore.GetCompaniesFromFragment("Google")).Name,
                "Should find 'Alphabet' (Google) company");
        }

        [TestMethod()]
        public void GetBoundaryTokensTest()
        {
            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple, twitterFeed));
            Assert.AreEqual(1, analyser.GetBoundaryTokens(0, 4).Count, "Should be a list with one item");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Google, twitterFeed));
            Assert.AreEqual(1, analyser.GetBoundaryTokens(16, 22).Count, "Should be a list with one item");
        }

        [TestMethod()]
        public void IsSingleNameTest()
        {
            Assert.IsTrue(analyser.IsSingleName("Blim"), "Should be true, because it is a single word");
            Assert.IsFalse(analyser.IsSingleName("Blim Blam"), "Should be false, because it is two words");
        }

        [TestMethod()]
        public void IsValidCompanyNameTest()
        {
            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple, twitterFeed));
            Assert.IsTrue(analyser.IsValidCompanyName("Apple", 0, PartOfSpeech.PROPN), "Should find 'Apple' a valid company name");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Google, twitterFeed));
            Assert.IsTrue(analyser.IsValidCompanyName("Google", 16, PartOfSpeech.PROPN), "Should find 'Google' a valid company name");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple, twitterFeed));
            Assert.IsFalse(analyser.IsValidCompanyName("Apple", 999, PartOfSpeech.NOUN), "Should NOT find 'Apple' a valid company name, because it was marked as NOUN");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Google, twitterFeed));
            Assert.IsFalse(analyser.IsValidCompanyName("Google", 999, PartOfSpeech.NOUN), "Should NOT find 'Google' a valid company name, because it was marked as NOUN");

            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Google, twitterFeed));
            Assert.IsFalse(analyser.IsValidCompanyName("Bananas", 0, PartOfSpeech.PROPN), "Should NOT find 'Bananas' a valid company name");
        }

        [TestMethod()]
        public async Task IsCompanyMentionedTestAsync()
        {
            _ = analyser.Analyse(new NewsItem("123", DateTimeOffset.Now, text_Apple, twitterFeed));

            SimpleCompany company1 = await companyDataStore.GetCompanyBySymbol("AAPL");
            Assert.IsTrue(analyser.IsCompanyMentioned(company1), "Should have found company (Apple [AAPL])");

            SimpleCompany company2 = await companyDataStore.GetCompanyBySymbol("GOOG");
            Assert.IsFalse(analyser.IsCompanyMentioned(company2), "Should NOT have found company (Google [GOOG])");
        }

        [TestMethod()]
        public void CleanTextTest()
        {
            Assert.AreEqual(
                "Apples are good",
                analyser.CleanText("Apples (particularly red ones) are    good - I want one"),
                "Should remove text in brackets and after hyphen and any big gaps");
            Assert.AreEqual(
                "Sanfilippo & Son Spice",
                analyser.CleanText("Sanfilippo & Son Ltd @Spice £ "),
                "Should remove words like Ltd and odd characters (keeping &) and any extra spaces left behind and trim");
        }

    }
}