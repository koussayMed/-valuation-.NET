using System.Text.Json.Serialization;

namespace VotePreferentiel.Models;

/// <summary>
/// Contenu du fichier JSON : la liste des bulletins de vote.
/// Format attendu :
/// {
///   "votes": [
///     { "voter": "Votant 1", "choices": ["Alice", "Bilal", "Chloé"] },
///     ...
///   ]
/// }
/// </summary>
public class VoteModel
{
    [JsonPropertyName("votes")]
    public List<Vote> Votes { get; set; } = new();
}

/// <summary>
/// Un bulletin : le votant et ses candidats classés par ordre de préférence
/// (le premier élément de <see cref="Choices"/> est le 1er choix).
/// </summary>
public class Vote
{
    [JsonPropertyName("voter")]
    public string Voter { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<string> Choices { get; set; } = new();
}
