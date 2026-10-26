using VotePreferentiel.Models;

namespace VotePreferentiel.Tests;

internal static class TestData
{
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

    public static IEnumerable<string[]> Repeat(int count, params string[] ballot) =>
        Enumerable.Repeat(ballot, count);
}
