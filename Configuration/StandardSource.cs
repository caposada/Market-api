using Elements;
using System.ComponentModel.DataAnnotations;

namespace Configuration
{
    public class StandardSource
    {
        [Key]
        public string FeedTitle { get; set; }
        public string FeedUrl { get; set; }
        public FeedType FeedType { get; set; }
        public string Timezone { get; set; }
    }
}
