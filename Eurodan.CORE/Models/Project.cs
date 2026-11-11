using Eurodan.Core.Enums;

namespace Eurodan.Core.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string ProjectNumber { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public FeeProposalStatus FeeProposalStatus { get; set; } = FeeProposalStatus.Pending;
        public string? ProjectManager { get; set; }
        public string? BuildingSurveyor { get; set; }
        public decimal? FeeProposal { get; set; }
        public CompletionStatus? PbdbStatus { get; set; }
        public CompletionStatus? FserStatus { get; set; }
        public decimal? InvoicedAmount { get; set; }
        public DateOnly? PaymentDueDate { get; set; }
        public decimal? OutstandingAmount { get; set; }
        public bool IsPaid =>
            InvoicedAmount.HasValue &&
            OutstandingAmount.HasValue && OutstandingAmount.Value == 0;
        public string? Comments { get; set; }
        public int ClientId { get; set; }
        public List<Building> Buildings { get; set; } = new List<Building>();
        public List<BuildingClass> BuildingClasses { get; set; } = new List<BuildingClass>();
        public List<TechnicalIssue> TechnicalIssues { get; set; } = new List<TechnicalIssue>();
    }
}
