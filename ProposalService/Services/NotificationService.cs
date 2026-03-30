using ProposalService.Models.DTOs;

namespace ProposalService.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private const string Url = "http://192.168.1.10:8077/notifications/send";

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
