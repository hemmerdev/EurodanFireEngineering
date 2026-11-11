namespace Eurodan.Core.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string? ContactName { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
