using System.Text.RegularExpressions;

namespace Elements
{
    public static class Utils
    {
        private static Regex hyphenRegex = new Regex(@"\s-\s.*", RegexOptions.Compiled);
        private static Regex bracketsRegex = new Regex(@"\(.*?\)", RegexOptions.Compiled);
        private static Regex textSplitRegex = new Regex(@"\s+", RegexOptions.Compiled);
        private static Regex hasLowercaseRegex = new Regex(@"[a-z]", RegexOptions.Compiled);
        private static Regex acronymsRegex = new Regex(@"([A-Z]+(?=\s))", RegexOptions.Compiled);
        private static Regex removeWordsRegex = new Regex(
            @"\b(the|limited|Ltd|group|inc|company|plc|stock|holdings|corp)\b",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsAllUppercase(string text)
        {
            return !hasLowercaseRegex.IsMatch(text);
        }

        public static string[] Acronyms(string text)
        {
            MatchCollection matches = acronymsRegex.Matches(text);
            return matches.Select(match => match.Value).ToArray();
        }

        public static string[] SplitText(string text)
        {
            return textSplitRegex.Split(text);
        }

        public static string Clean(string text)
        {
            // Use REGEX to remove
            // stuff in brackets: '(blah blah)' 
            text = bracketsRegex.Replace(text, "");
            // hyphen and then gumpth: ' - Class ..."
            text = hyphenRegex.Replace(text, "");

            text = RemoveMultipleSpaces(text);

            return text;
        }

        public static string Reduce(string text)
        {
            // Now remove whole words like:
            // The, Limited, Ltd, Group, Inc
            text = removeWordsRegex.Replace(text, "");

            // Clean up odd characters
            text = new string((from c in text
                               where char.IsWhiteSpace(c)
                               || char.IsLetterOrDigit(c)
                               || c == '&'                  // e.g. Sanfilippo & Son
                               select c
            ).ToArray());

            text = RemoveMultipleSpaces(text);
            text = text.Trim();

            return text;
        }

        public static string RemoveMultipleSpaces(string text)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(text, " ");
        }

        public static int GetWhen(DateTime scheduledTime, DateTime? thisTime = null)
        {
            thisTime = thisTime ?? DateTime.Now;
            return ((int)(scheduledTime - thisTime.Value).TotalMilliseconds);
        }
    }
}
