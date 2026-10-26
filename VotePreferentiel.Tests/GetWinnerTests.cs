using VotePreferentiel.Models;
using VotePreferentiel.Services;

namespace VotePreferentiel.Tests;

public class GetWinnerTests
{
    private readonly IVoteCounter _counter = new VoteCounter();

    // 1. Cas nominal : l'exemple du sujet -> Chloé.
    [Test]
    public async Task CasNominal_ExempleDuSujet_ChloeGagne()
    {
        var model = TestData.CreateModel(
        [
            ["Alice", "Bilal", "Chloé"],
            ["Alice", "Chloé", "Bilal"],
            ["Bilal", "Chloé", "Alice"],
            ["Chloé", "Bilal", "Alice"],
            ["Chloé", "Alice", "Bilal"]
        ]);

        var winner = _counter.GetWinner(model);
        Console.WriteLine($"Gagnant(e) : {winner}");

    }

    // 2. Un seul candidat : un bulletin ["Alice"] -> Alice.
    [Test]
    public async Task UnSeulCandidat_AliceGagne()
    {
        var model = TestData.CreateModel([["Alice"]]);

        var winner = _counter.GetWinner(model);
        Console.WriteLine($"Gagnant(e) : {winner}");

    }

    // 3. Report des préférences : Diane éliminée au 1er tour, Bilal au 2e -> Chloé.
    [Test]
    public async Task ReportDesPreferences_ChloeGagne()
    {
        var model = TestData.CreateModel(
        [
            .. TestData.Repeat(4, "Alice", "Bilal", "Chloé", "Diane"),
            .. TestData.Repeat(3, "Bilal", "Chloé", "Alice", "Diane"),
            .. TestData.Repeat(3, "Chloé", "Alice", "Bilal", "Diane"),
            .. TestData.Repeat(1, "Diane", "Chloé", "Bilal", "Alice")
        ]);

        var winner = _counter.GetWinner(model);
        Console.WriteLine($"Gagnant(e) : {winner}");

        await Assert.That(winner).IsEqualTo("Chloé");
    }

    // 4. Égalité en dernière position : Alice et Bilal éliminés simultanément -> Chloé.
    [Test]
    public async Task EgaliteEnDernierePosition_ChloeGagne()
    {
        var model = TestData.CreateModel(
        [
            ["Alice", "Chloé", "Bilal"],
            ["Bilal", "Chloé", "Alice"],
            ["Chloé", "Alice", "Bilal"],
            ["Chloé", "Bilal", "Alice"]
        ]);

        var winner = _counter.GetWinner(model);
        Console.WriteLine($"Gagnant(e) : {winner}");

        await Assert.That(winner).IsEqualTo("Chloé");
    }

    // 5. Bulletin vide : aucune préférence -> exception.
    [Test]
    public async Task BulletinVide_LeveUneException()
    {
        var model = TestData.CreateModel([[]]);

        var exception = await Assert.That(() => _counter.GetWinner(model))
            .Throws<InvalidOperationException>();
        Console.WriteLine($"Exception levée : {exception!.Message}");
    }
}