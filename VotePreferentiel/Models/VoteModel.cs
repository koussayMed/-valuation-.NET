using System.Text.Json.Serialization;

namespace VotePreferentiel.Models;

public class VoteModel
{
    [JsonPropertyName("votes")]
    public List<Vote> Votes { get; set; } = new();
}

public class Vote
{
    [JsonPropertyName("voter")]
    public string Voter { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<string> Choices { get; set; } = new();
}
