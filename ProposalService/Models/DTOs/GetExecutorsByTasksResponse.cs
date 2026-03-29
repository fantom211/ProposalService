namespace WorkService.Models.DTOs
{
    public class GetExecutorsByTasksResponse
    {
        public Dictionary<Guid, List<Guid>> Data { get; set; } = new();
    }
}
