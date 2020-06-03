using LiteDB;

namespace Highwind.Models
{
    public class Client : ClientRequest
    {
        public int Id { get; set; }
        public string ApiKey { get; set; }
        public bool IsInactive { get; set; }
    }
}