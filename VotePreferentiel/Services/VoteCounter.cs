using VotePreferentiel.Models;

namespace VotePreferentiel.Services;

/// <summary>
/// Calcule le résultat d'un vote préférentiel (élimination successive + report des voix).
/// </summary>
public class VoteCounter : IVoteCounter
{
    public HashSet<string> GetCandidates(VoteModel voteModel)
    {
        ArgumentNullException.ThrowIfNull(voteModel);

        return voteModel.Votes
            .SelectMany(vote => vote.Choices)
            .Where(choice => !string.IsNullOrWhiteSpace(choice))
            .ToHashSet();
    }

    public List<string> GetAllFirstChoices(VoteModel voteModel)
    {
        ArgumentNullException.ThrowIfNull(voteModel);

        return voteModel.Votes
            .Select(vote => vote.Choices.FirstOrDefault(choice => !string.IsNullOrWhiteSpace(choice)))
            .Where(choice => choice is not null)
            .Select(choice => choice!)
            .ToList();
    }

    public IDictionary<string, int> CountFirstChoices(List<string> firstChoices)
    {
        ArgumentNullException.ThrowIfNull(firstChoices);

        return firstChoices
            .GroupBy(candidate => candidate)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    public string[] GetCandidatesWithLeastVotes(IDictionary<string, int> firstChoiceCounts)
    {
        ArgumentNullException.ThrowIfNull(firstChoiceCounts);

        if (firstChoiceCounts.Count == 0)
            return Array.Empty<string>();

        var minimum = firstChoiceCounts.Values.Min();

        return firstChoiceCounts
            .Where(pair => pair.Value == minimum)
            .Select(pair => pair.Key)
            .OrderBy(candidate => candidate)
            .ToArray();
    }

    public string GetWinner(VoteModel voteModel)
    {
        ArgumentNullException.ThrowIfNull(voteModel);

        // Candidats encore en lice.
        var remaining = GetCandidates(voteModel);

        if (remaining.Count == 0)
            throw new InvalidOperationException("Aucun choix exprimé : impossible de déterminer un gagnant.");

        while (remaining.Count > 1)
        {
            // Report des voix : sur chaque bulletin, on ne garde que les candidats encore en lice.
            // Le 1er candidat restant devient donc le « premier choix » de ce tour.
            var currentRound = new VoteModel
            {
                Votes = voteModel.Votes
                    .Select(vote => new Vote
                    {
                        Voter = vote.Voter,
                        Choices = vote.Choices.Where(remaining.Contains).ToList()
                    })
                    .ToList()
            };

            var counts = CountFirstChoices(GetAllFirstChoices(currentRound));

            // Un candidat en lice sans aucune voix compte pour 0 (il doit pouvoir être éliminé).
            foreach (var candidate in remaining)
            {
                if (!counts.ContainsKey(candidate))
                    counts[candidate] = 0;
            }

            var losers = GetCandidatesWithLeastVotes(counts);

            // Si tous les candidats restants sont à égalité, les éliminer tous ne laisserait personne.
            if (losers.Length == remaining.Count)
                throw new InvalidOperationException(
                    $"Égalité parfaite entre {string.Join(", ", losers)} : impossible de départager.");

            remaining.ExceptWith(losers);
        }

        return remaining.Single();
    }
}
