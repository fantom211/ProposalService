using ProposalService.Models.DTOs;

namespace ProposalService.Services
{
    public class NotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        public NotificationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendNotificationAsync(NotificationDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/notification/notifications/send", 
                dto);
            response.EnsureSuccessStatusCode();
        }

    }
}
