namespace ProposalService.Models.DTOs
{
    public class ProposalDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid ExecutorId { get; set; }
        public string Status { get; set; } // pending, accepted, rejected
    }
}
