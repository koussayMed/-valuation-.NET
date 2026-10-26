using System.Net;
using System.Text;
using VotePreferentiel.Services;

namespace VotePreferentiel.Tests;

/// <summary>
/// Tests complémentaires (bonus) du 3.3 : VoteProvider désérialise bien le JSON,
/// sans appeler Internet (on remplace le réseau par un faux HttpMessageHandler).
/// </summary>
public class VoteProviderTests
{
    private const string Json = """
        {
          "votes": [
            { "voter": "Votant 1", "choices": ["Alice", "Bilal", "Chloé"] },
            { "voter": "Votant 2", "choices": ["Alice", "Chloé", "Bilal"] }
          ]
        }
        """;

    [Test]
    public async Task GetVotesAsync_DeserialiseLeJson()
    {
        var httpClient = new HttpClient(new FakeHandler(Json));
        var provider = new VoteProvider(httpClient);

        var model = await provider.GetVotesAsync("https://exemple.test/votes.json");

        await Assert.That(model.Votes.Count).IsEqualTo(2);
        await Assert.That(model.Votes[0].Voter).IsEqualTo("Votant 1");
        await Assert.That(model.Votes[1].Choices[1]).IsEqualTo("Chloé");
    }

    [Test]
    public async Task Deserialize_AccepteUnTableauDeTableaux()
    {
        var model = VoteProvider.Deserialize("""[["Alice","Bilal"],["Bilal","Alice"]]""");

        await Assert.That(model.Votes.Count).IsEqualTo(2);
        await Assert.That(model.Votes[1].Choices[0]).IsEqualTo("Bilal");
    }

    private sealed class FakeHandler(string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "text/plain")
            };
            return Task.FromResult(response);
        }
    }
}
