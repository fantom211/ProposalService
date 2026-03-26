namespace ProposalService.Models.Entities
{
    public class Proposal
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskId { get; set; }
        public Guid ExecutorId { get; set; }
        public string Status { get; set; } // PENDING, ACCEPTED, REJECTED, CANCELLED
    }
}
