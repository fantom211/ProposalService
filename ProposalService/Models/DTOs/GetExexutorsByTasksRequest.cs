namespace WorkService.Models.DTOs
{
    public class GetExecutorsByTasksRequest
    {
        public List<Guid> TaskIds { get; set; } = new();
    }
}
