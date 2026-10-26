using VotePreferentiel.Services;

namespace VotePreferentiel.Worker;

public class VoteWorker : BackgroundService
{
    private readonly ILogger<VoteWorker> _logger;
    private readonly IVoteProvider _voteProvider;
    private readonly IVoteCounter _voteCounter;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _lifetime;

    public VoteWorker(
        ILogger<VoteWorker> logger,
        IVoteProvider voteProvider,
        IVoteCounter voteCounter,
        IConfiguration configuration,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _voteProvider = voteProvider;
        _voteCounter = voteCounter;
        _configuration = configuration;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var url = _configuration["VoteSettings:Url"]
                      ?? throw new InvalidOperationException("VoteSettings:Url manquant dans appsettings.json.");

            _logger.LogInformation("Téléchargement des votes depuis {Url}", url);
            var votes = await _voteProvider.GetVotesAsync(url, stoppingToken);

            // --- 3.4 : vérifier que VoteProvider retourne correctement les données ---
            Console.WriteLine($"{votes.Votes.Count} bulletin(s) récupéré(s) :");
            foreach (var vote in votes.Votes)
                Console.WriteLine($"  {vote.Voter,-10} : {string.Join(" > ", vote.Choices)}");

            // --- 3.5 : calcul du gagnant ---
            var candidates = _voteCounter.GetCandidates(votes);
            Console.WriteLine($"Candidats : {string.Join(", ", candidates)}");

            var firstRound = _voteCounter.CountFirstChoices(_voteCounter.GetAllFirstChoices(votes));
            Console.WriteLine("Premier tour : " +
                              string.Join(", ", firstRound.Select(pair => $"{pair.Key} = {pair.Value}")));

            var winner = _voteCounter.GetWinner(votes);
            Console.WriteLine();
            Console.WriteLine($"Gagnant(e) de l'élection : {winner}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Erreur lors du traitement des votes");
            Environment.ExitCode = 1;
        }
        finally
        {
            // Le traitement est ponctuel : on arrête l'application une fois terminé.
            _lifetime.StopApplication();
        }
    }
}
