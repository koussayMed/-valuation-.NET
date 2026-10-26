using VotePreferentiel.Models;

namespace VotePreferentiel.Tests;

internal static class TestData
{
    /// <summary>Construit un VoteModel à partir d'une liste de bulletins.</summary>
    public static VoteModel CreateModel(IEnumerable<string[]> ballots) => new()
    {
        Votes = ballots
            .Select((choices, index) => new Vote
            {
                Voter = $"Votant {index + 1}",
                Choices = choices.ToList()
            })
            .ToList()
    };

    /// <summary>Répète un même bulletin <paramref name="count"/> fois.</summary>
    public static IEnumerable<string[]> Repeat(int count, params string[] ballot) =>
        Enumerable.Repeat(ballot, count);
}
