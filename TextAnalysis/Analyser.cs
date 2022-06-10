using Catalyst;
using Catalyst.Models;
using Companies;
using Elements;
using Mosaik.Core;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("TextAnalysisTests")]
namespace TextAnalysis
{
    public class Analyser
    {
        const string STOCKMARKETENTITY = "StockMarketCompany";
        const string ORGANISATIONSENTITY = "Organization";
        const int MIN_TEXT_ELEMENT_SIZE = 3;
        const int MIN_SYMBOL_SIZE = 2;
        const int MIN_UPERCASE_COMPANYNAME_SIZE = 2; // e.g. HP
        const int MAX_UPERCASE_COMPANYNAME_SIZE = 6; // A company name that is capilized wouldn't be so weird?

        private static Pipeline? nlp;
        private CompanyDataStore companyDataStore;

        public AnalysisInfo Info { get; private set; }

        public Analyser(CompanyDataStore companyDataStore)
        {
            this.companyDataStore = companyDataStore;
        }

        public async Task Analyse(NewsItem newsItem)
        {
            this.Info = new AnalysisInfo(newsItem);
            await Setup();
            await AnalyseNaturalanguage();
        }

        private async Task AnalyseNaturalanguage()
        {
            Document doc;

            string text = this.Info.Text;

            doc = new Document(text, Language.English);
            nlp.ProcessSingle(doc);

            Breakdown(doc);

            await CheckForSymbols();


            bool hasFinding = await AnalyseForCompaniesAsync(doc);
            if (!hasFinding)
            {
                // No companies found, so try cleaning up text nd try again
                string cleanedText = CleanText(text);
                doc = new Document(cleanedText, Language.English);
                nlp.ProcessSingle(doc);
                await AnalyseForCompaniesAsync(doc);
            }
        }

        private void Breakdown(Document doc)
        {
            // Lets breakdown the text into its parts of speech
            List<AnalysisBreakDown.Span> breakdownSpans = new List<AnalysisBreakDown.Span>();
            int position = 0;
            foreach (var span in doc.Spans)
            {
                AnalysisBreakDown.Span breakdownSpan = new AnalysisBreakDown.Span();
                foreach (var token in span.Tokens)
                {
                    AnalysisBreakDown.Token breakdownToken = new AnalysisBreakDown.Token(
                        token.Value,
                        token.POS,
                        token.Lemma,
                        position,
                        token.Begin,
                        token.Length);
                    breakdownSpan.Add(breakdownToken);
                    position++;
                }
                breakdownSpans.Add(breakdownSpan);
            }
            Info.SetAnalysisBreakDown(new AnalysisBreakDown(breakdownSpans));
        }

        internal async Task CheckForSymbols()
        {
            bool hasFinding = false;

            List<AnalysisBreakDown.Token> breakdownTokens = Info.BreakDown.AllTokens;
            foreach (var breakdownToken in breakdownTokens)
            {
                string text = breakdownToken.Text;
                if (!Utils.IsAllUppercase(text))
                    continue; // Symbols are always in full capitals

                SimpleCompany? company = null;
                SimpleCompany? bestCompany = null;

                // Check if it's a symbol
                if (breakdownToken.Position > 0 && breakdownTokens[breakdownToken.Position - 1].Text == "$")
                {
                    // If from Twitter, some people like to put a dollar symbol in front of a market symbol
                    // e.g. $SBUX for Starbucks
                    // If not the first position, check if the previous token was the '$' symbol
                    company = await companyDataStore.GetCompanyBySymbol(text);
                    bestCompany = company;
                }
                else if (breakdownToken.POS == PartOfSpeech.PROPN && text.Length >= MIN_SYMBOL_SIZE)
                {
                    company = await companyDataStore.GetCompanyBySymbol(text);
                    if (company != null)
                    {
                        if (IsCompanyMentioned(company))
                        {
                            // Okay the symbol is mentioned and we may have found a company
                            // with that symbol, but if the symbol is there in the artcle,
                            // then surely the company name (or part of the name) is mentioned!
                            bestCompany = company;
                        }
                        else if (breakdownToken.Position == 0)
                        {
                            // It's capitalised and at the beginning of the sentence, so lets assume it's a symbol
                            bestCompany = company;
                        }
                    }
                }

                if (bestCompany != null)
                {
                    List<AnalysisBreakDown.Token> boundaryTokens = new List<AnalysisBreakDown.Token>();
                    boundaryTokens.Add(breakdownToken);
                    Info.AddCompanyFinding(
                        bestCompany,
                        AnalysisConfidence.HIGH,
                        AnalysisRationale.SYMBOL,
                        boundaryTokens);
                    hasFinding = true;
                }
            }
        }

        private async Task<bool> AnalyseForCompaniesAsync(Document doc)
        {
            bool hasFinding = false;

            // See if an entity token is a company
            List<ITokens> allEntityTokens = doc.SelectMany(span => span.GetEntities()).ToList();
            List<ITokens> relevantEntityTokens = allEntityTokens.FindAll(
                token => token.EntityType.Type == STOCKMARKETENTITY
                || token.EntityType.Type == ORGANISATIONSENTITY);
            foreach (var token in relevantEntityTokens)
            {
                string text = Utils.Clean(token.Value);
                SimpleCompany? bestCompany = null;
                AnalysisConfidence bestConfidence = AnalysisConfidence.LOW;
                AnalysisRationale bestRationale = AnalysisRationale.NONE;
                SimpleCompany? company = null;

                if (Utils.IsAllUppercase(text))
                {
                    // e.g. like HP, UBS, etc. or the SYMBOL

                    // Check if it's part of a company name (e.g. HP)
                    if (text.Length >= MIN_UPERCASE_COMPANYNAME_SIZE
                        && text.Length < MAX_UPERCASE_COMPANYNAME_SIZE)
                    {
                        company = await companyDataStore.GetCompanyByName(text); // Probability: High
                        if (company != null && bestConfidence <= AnalysisConfidence.HIGH)
                        {
                            bestCompany = company;
                            bestConfidence = AnalysisConfidence.HIGH;
                            bestRationale = AnalysisRationale.FULL_NAME;
                        }
                    }
                }
                else if (text.Length >= MIN_TEXT_ELEMENT_SIZE)
                {
                    company = await companyDataStore.GetCompany(text); // Probability: High
                    if (company != null && bestConfidence <= AnalysisConfidence.HIGH)
                    {
                        if (IsSingleName(text))
                        {
                            // Good that we have found a company with this word, but is this word
                            // the company name or just part of a company name, e.g.
                            // 'Acrysil to acquire UK's Tickford Orange Ltd for 11 million pounds'
                            // Might find 'Orange' as a company name, which is clearly wrong
                            if (IsValidCompanyName(text, token.Begin, token.POS))
                            {
                                bestCompany = company;
                                bestConfidence = AnalysisConfidence.HIGH;
                                bestRationale = AnalysisRationale.FULL_NAME;
                            }
                        }
                        else
                        {
                            // Found text is multi worded, making it like to be a real name
                            bestCompany = company;
                            bestConfidence = AnalysisConfidence.HIGH;
                            bestRationale = AnalysisRationale.FULL_NAME;
                        }
                    }

                    if (company == null)
                    {
                        // Maybe we have just the fragment of the company name in this token
                        // e.g. 'Lululemon' = Lululemon Athletica Inc
                        List<SimpleCompany> companies = await companyDataStore.GetCompaniesFromFragment(text); // Probability: Low
                        company = HasFragment(text, token.Begin, token.POS, companies);
                        if (company != null)
                        {
                            if (company != null && bestConfidence <= AnalysisConfidence.LOW)
                            {
                                bestCompany = company;
                                bestConfidence = AnalysisConfidence.MEDIUM;
                                bestRationale = AnalysisRationale.NAME_FRAGMENT;
                            }
                        }
                    }
                }



                // *** another if ... Need another for XXX:NYSE type
                // find with = AnalysisConfidence: High + AnalysisRationale.SYMBOL_MARKET


                if (bestCompany != null)
                {
                    Info.AddCompanyFinding(
                        bestCompany,
                        bestConfidence,
                        bestRationale,
                        GetBoundaryTokens(token.Begin, token.End));
                    hasFinding = true;
                }
            }

            return hasFinding;
        }

        internal SimpleCompany? HasFragment(string text, int begin, PartOfSpeech pos, List<SimpleCompany> companies)
        {
            foreach (var company in companies)
            {
                if (IsValidCompanyName(text, begin, pos))
                {
                    return company;
                }
            }

            return null;
        }

        internal List<AnalysisBreakDown.Token> GetBoundaryTokens(int begin, int end)
        {
            List<AnalysisBreakDown.Token> boundaryTokens = new List<AnalysisBreakDown.Token>();
            foreach (var breakdownToken in Info.BreakDown.AllTokens)
            {
                int breakdownBegin = breakdownToken.Begin;
                int breakdownEnd = breakdownToken.Begin + breakdownToken.Length - 1;
                if (breakdownBegin >= begin && breakdownEnd <= end)
                {
                    boundaryTokens.Add(breakdownToken);
                }
            }
            return boundaryTokens;
        }

        internal bool IsSingleName(string text)
        {
            return Utils.SplitText(text).Count() > 1 ? false : true;
        }

        internal bool IsValidCompanyName(string text, int begin, PartOfSpeech pos)
        {
            var breakdownToken = Info.BreakDown.AllTokens.Find(x => x.Begin == begin);
            if (breakdownToken != null)
            {
                if (breakdownToken.POS == PartOfSpeech.PROPN)
                {
                    if (breakdownToken.Position > 0)
                    {
                        var previousToken = Info.BreakDown.AllTokens[breakdownToken.Position - 1];
                        if (previousToken.POS == PartOfSpeech.PROPN)
                        {
                            // Previous token was also a Proper Noun and probably part
                            // of the company name, so why didn't it find that?
                            // Therefore this is not a valid company name
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            foreach (var span in Info.BreakDown.Spans)
            {
                var firstToken = span[0];
                if (firstToken.Text == text)
                {
                    // This potential name is at the beginning of a
                    // news item sentence so is valid in that sense

                    // But is it a proper noun?
                    if (pos == PartOfSpeech.PROPN)
                        return true;
                }
            }

            return false;
        }

        internal bool IsCompanyMentioned(SimpleCompany company)
        {
            string wholeText = Info.Text;
            string[] splitWholeText = Utils.SplitText(wholeText);
            string[] splitCompanyName = Utils.SplitText(company.Name);
            for (int splitIndex = 0; splitIndex < splitCompanyName.Length; splitIndex++)
            {
                int partIndex = splitWholeText.FindIndex(0, x => x == splitCompanyName[splitIndex]);
                if (partIndex == 0 && splitIndex == 0)
                {
                    // First word of text and first word of company name the same, likely to mean good
                    return true;
                }
                else if (partIndex > 0)
                {
                    // Something found
                    if (splitIndex > 0)
                    {
                        // Something found, but it's not the company's first name,
                        // but it isn't the first word of the text, so is the
                        // previous word of the company name the same as the text
                        // part's previous word?
                        if (splitCompanyName[splitIndex - 1] == splitWholeText[partIndex - 1])
                        {
                            // The second and first name of the company are
                            // mentioned in the right order
                            return true;
                        }

                    }
                }
            }
            return false;
        }

        internal string CleanText(string text)
        {
            text = Utils.Clean(text);
            text = Utils.Reduce(text);

            return text;
        }

        private async Task Setup()
        {
            if (nlp == null)
            {
                Mosaik.Core.Storage.Current = new DiskStorage("catalyst-models");
                Catalyst.Models.English.Register(); //You need to pre-register each language (and install the respective NuGet Packages)
                nlp = await Pipeline.ForAsync(Language.English);
                nlp.Add(await AveragePerceptronEntityRecognizer.FromStoreAsync(
                    language: Language.English, version: Mosaik.Core.Version.Latest, tag: "WikiNER"));

                //For correcting Entity Recognition mistakes, you can use the Neuralyzer class. 
                //This class uses the Pattern Matching entity recognition class to perform "forget-entity"
                //and "add-entity" 
                //passes on the document, after it has been processed by all other proceses in the NLP pipeline
                var neuralizer = new Neuralyzer(Language.English, 0, "WikiNER-sample-fixes");
                nlp.UseNeuralyzer(neuralizer);

                var spotter = new Spotter(Language.Any, 0, "company", STOCKMARKETENTITY);
                spotter.Data.IgnoreCase = false;
                List<string> flatlist = await companyDataStore.GetFlatList();
                spotter.AppendList(flatlist);
                nlp.Add(spotter);
            }
        }

        private static void PrintDocumentEntities(IDocument doc)
        {
            Console.WriteLine($"Input text:\n\t'{doc.Value}'\n\nTokenized Value:\n\t'{doc.TokenizedValue(mergeEntities: true)}'\n\nEntities: \n{string.Join("\n", doc.SelectMany(span => span.GetEntities()).Select(e => $"\t{e.Value} [{e.EntityType.Type}]"))}");
        }


    }


}