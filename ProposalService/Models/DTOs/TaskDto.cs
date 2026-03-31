namespace ProposalService.Models.DTOs
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}
