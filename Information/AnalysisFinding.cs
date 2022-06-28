using Elements;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TextAnalysis;

namespace Information
{
    public class AnalysisFinding
    {
        [Key]
        public Guid Id { get; set; }
        public Guid NewsItemId { get; set; }
        public AnalysisConfidence Confidence { get; set; }
        public AnalysisRationale Rationale { get; set; }
        public string CompanyJson { get; set; }
        public string TokensJson { get; set; }

        [NotMapped]
        public List<TextAnalysis.AnalysisBreakDown.Token> Tokens
        {
            get
            {
                return JsonSerializer.Deserialize<List<TextAnalysis.AnalysisBreakDown.Token>>(TokensJson);
            }
        }
        [NotMapped]
        public BasicCompany Company
        {
            get
            {
                return JsonSerializer.Deserialize<BasicCompany>(CompanyJson);
            }
        }

        public AnalysisFinding(
            Guid id,
            Guid newsItemId,
            string companyJson,
            AnalysisConfidence confidence,
            AnalysisRationale rationale,
            string tokensJson)
        {
            this.Id = id;
            this.NewsItemId = newsItemId;
            this.CompanyJson = companyJson;
            this.Confidence = confidence;
            this.Rationale = rationale;
            this.TokensJson = tokensJson;
        }

        public AnalysisFinding(
            Guid newsItemId,
            BasicCompany company,
            AnalysisConfidence confidence,
            AnalysisRationale rationale,
            List<TextAnalysis.AnalysisBreakDown.Token> tokens)
        {
            this.Id = Guid.NewGuid();
            this.NewsItemId = newsItemId;
            this.Confidence = confidence;
            this.Rationale = rationale;
            this.CompanyJson = JsonSerializer.Serialize<BasicCompany>(company);
            this.TokensJson = JsonSerializer.Serialize<List<TextAnalysis.AnalysisBreakDown.Token>>(tokens);
        }
    }
}
