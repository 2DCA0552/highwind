using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Highwind.Models
{
    public class ClientRequest {
        [Required]
        public string Audience { get; set; }
        [Required]
        public string Application { get; set; }
        public string CookieDomain { get; set; }
        public string CookiePath { get; set; }
        public string TokenName { get; set; }
        public List<string> AppGroupRegexes { get; set; }
    }
}