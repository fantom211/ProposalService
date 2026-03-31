using ProposalService.Models.DTOs;

namespace ProposalService.Services
{
    public class WorkServiceClient
    {
        private readonly HttpClient _httpClient;
        public WorkServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TaskDto?> GetTask(Guid taskId)
        {
            return await _httpClient.GetFromJsonAsync<TaskDto>($"/api/tasks/{taskId}");
        }

        public async Task NotifyProposalAccepted(Guid taskId)
        {
            await _httpClient.PostAsJsonAsync(
                "/api/tasks/proposal-accepted",
                new ProposalAcceptedDto { TaskId = taskId });
        }
    }
}
