using Catalyst;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextAnalysis
{
    public class AnalysisBreakDown
    {
        public class Span : List<Token>
        {

            //public 

        }

        public class Token
        {
            public string Text { get; }
            public PartOfSpeech POS { get; }
            public string Lemma { get; }
            public int Position { get; }
            public int Begin { get; }
            public int Length { get; }

            public Token(string text, PartOfSpeech pos, string lemma, int position, int begin, int length)
            {
                this.Text = text;
                this.POS = pos;
                this.Lemma = lemma;
                this.Position = position;
                this.Begin = begin;
                this.Length = length;
            }

            public override string ToString()
            {
                return $"{Text} ({POS})";
            }
        }

        [NotMapped]
        public List<Token> AllTokens
        {
            get
            {
                return Spans.SelectMany(span => span).ToList<Token>();
            }
        }

        public List<Span> Spans { get; }

        public AnalysisBreakDown(List<Span> spans)
        {
            this.Spans = spans;
        }

    }
}
