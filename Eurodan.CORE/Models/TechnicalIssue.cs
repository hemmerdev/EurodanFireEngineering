namespace Eurodan.Core.Models
{
    public class TechnicalIssue
    {
        public int TechnicalIssueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
