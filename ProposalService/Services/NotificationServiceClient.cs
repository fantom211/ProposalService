using ProposalService.Models.DTOs;

namespace ProposalService.Services
{
    public class NotificationServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly bool _enabled;
        public NotificationServiceClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _enabled = config.GetValue<bool>("Services:NotificationServiceEnabled");
        }

        public async Task SendNotificationAsync(NotificationDto dto)
        {
            if (!_enabled) return;

            var response = await _httpClient.PostAsJsonAsync(
                "notification/notifications/send",
                dto);
            response.EnsureSuccessStatusCode();
        }

        //Заглушка
        //public async Task SendNotificationAsync(NotificationDto dto)
        //{
        //    if (!_enabled)
        //        return;

        //    var response = await _httpClient.PostAsJsonAsync(
        //        "/notifications/send",
        //        dto);
        //    response.EnsureSuccessStatusCode();
        //}

    }
}
