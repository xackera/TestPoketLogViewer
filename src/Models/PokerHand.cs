using System.Text.Json.Serialization;

namespace TestPoketLogViewer.Models
{
    public class PokerHand
    {
        [JsonPropertyName("HandID")]
        public long HandId { get; set; }

        [JsonPropertyName("TableName")]
        public string? TableName { get; set; }

        [JsonPropertyName("Players")]
        public List<string>? Players { get; set; }

        [JsonPropertyName("Winners")]
        public List<string>? Winners { get; set; }

        [JsonPropertyName("WinAmount")]
        public string? WinAmount { get; set; }
    }
}
