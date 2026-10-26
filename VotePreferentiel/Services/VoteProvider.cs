using System.Text.Json;
using VotePreferentiel.Models;

namespace VotePreferentiel.Services;


public class VoteProvider : IVoteProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public VoteProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<VoteModel> GetVotesAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("L'URL du fichier de votes est vide.", nameof(url));

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        // GitHub "raw" renvoie text/plain : on lit la chaîne puis on désérialise nous-mêmes.
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return Deserialize(json);
    }

    public static VoteModel Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("Le fichier de votes est vide.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object)
        {
            return JsonSerializer.Deserialize<VoteModel>(json, JsonOptions)
                   ?? throw new InvalidOperationException("Impossible de désérialiser le fichier de votes.");
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            var firstIsArray = root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.Array;

            if (firstIsArray)
            {
                var ballots = JsonSerializer.Deserialize<List<List<string>>>(json, JsonOptions) ?? new();
                return new VoteModel
                {
                    Votes = ballots.Select((choices, i) => new Vote { Voter = $"Votant {i + 1}", Choices = choices }).ToList()
                };
            }

            var votes = JsonSerializer.Deserialize<List<Vote>>(json, JsonOptions) ?? new();
            return new VoteModel { Votes = votes };
        }

        throw new InvalidOperationException("Format JSON non reconnu pour le fichier de votes.");
    }
}
