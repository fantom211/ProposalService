namespace ProposalService.Models.DTOs
{
    public class CreateProposalDto
    {
        public Guid TaskId { get; set; }
        public Guid ExecutorId { get; set; }
    }
}
