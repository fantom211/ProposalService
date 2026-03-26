using System.Text.Json.Serialization;

namespace ProposalService.Models.Entities
{
    public class Recipient
    {
        [JsonPropertyName("id")]
        public Guid UserId { get; set; }
    }
}
