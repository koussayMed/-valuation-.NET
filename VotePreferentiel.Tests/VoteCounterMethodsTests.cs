using VotePreferentiel.Models;
using VotePreferentiel.Services;

namespace VotePreferentiel.Tests;

/// <summary>
/// Tests complémentaires (bonus) : chaque méthode intermédiaire de VoteCounter,
/// pour vérifier le 3.5 morceau par morceau.
/// </summary>
public class VoteCounterMethodsTests
{
    private readonly IVoteCounter _counter = new VoteCounter();

    private static VoteModel Exemple() => TestData.CreateModel(
    [
        ["Alice", "Bilal", "Chloé"],
        ["Alice", "Chloé", "Bilal"],
        ["Bilal", "Chloé", "Alice"],
        ["Chloé", "Bilal", "Alice"],
        ["Chloé", "Alice", "Bilal"]
    ]);

    [Test]
    public async Task GetCandidates_RetourneLesTroisCandidats()
    {
        var candidates = _counter.GetCandidates(Exemple());

        await Assert.That(candidates.Count).IsEqualTo(3);
        await Assert.That(candidates.Contains("Alice")).IsTrue();
        await Assert.That(candidates.Contains("Bilal")).IsTrue();
        await Assert.That(candidates.Contains("Chloé")).IsTrue();
    }

    [Test]
    public async Task GetAllFirstChoices_RetourneLePremierChoixDeChaqueVotant()
    {
        var firstChoices = _counter.GetAllFirstChoices(Exemple());

        await Assert.That(string.Join(",", firstChoices)).IsEqualTo("Alice,Alice,Bilal,Chloé,Chloé");
    }

    [Test]
    public async Task CountFirstChoices_CompteLesVoix()
    {
        var counts = _counter.CountFirstChoices(["Alice", "Alice", "Bilal", "Chloé", "Chloé"]);

        await Assert.That(counts["Alice"]).IsEqualTo(2);
        await Assert.That(counts["Bilal"]).IsEqualTo(1);
        await Assert.That(counts["Chloé"]).IsEqualTo(2);
    }

    [Test]
    public async Task GetCandidatesWithLeastVotes_RetourneBilal()
    {
        var counts = new Dictionary<string, int> { ["Alice"] = 2, ["Bilal"] = 1, ["Chloé"] = 2 };

        var losers = _counter.GetCandidatesWithLeastVotes(counts);

        await Assert.That(string.Join(",", losers)).IsEqualTo("Bilal");
    }

    [Test]
    public async Task GetCandidatesWithLeastVotes_EgaliteRetourneLesDeux()
    {
        var counts = new Dictionary<string, int> { ["Alice"] = 1, ["Bilal"] = 1, ["Chloé"] = 2 };

        var losers = _counter.GetCandidatesWithLeastVotes(counts);

        await Assert.That(string.Join(",", losers)).IsEqualTo("Alice,Bilal");
    }
}
