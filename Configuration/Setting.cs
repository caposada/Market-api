using System.ComponentModel.DataAnnotations;

namespace Configuration
{
    public class Setting
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
