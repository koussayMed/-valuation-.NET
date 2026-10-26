using VotePreferentiel.Models;

namespace VotePreferentiel.Services;

public interface IVoteProvider
{
    /// <summary>Télécharge le fichier JSON à l'URL donnée et le désérialise.</summary>
    Task<VoteModel> GetVotesAsync(string url, CancellationToken cancellationToken = default);
}
