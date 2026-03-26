using ProposalService.Models.DTOs;

namespace ProposalService.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private const string Url = "https://4595-192-124-209-165.ngrok-free.app/notification/notifications/send";

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendNotificationAsync(NotificationDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(Url, dto);
            response.EnsureSuccessStatusCode();
        }

    }
}
