using VotePreferentiel.Models;

namespace VotePreferentiel.Services;

public interface IVoteCounter
{
    // Récupère la liste des candidats présents dans les votes.
    HashSet<string> GetCandidates(VoteModel voteModel);

    // Détermine le gagnant à partir des choix des votants.
    string GetWinner(VoteModel voteModel);

    // Récupère le premier choix de chaque votant.
    List<string> GetAllFirstChoices(VoteModel voteModel);

    // Compte les premières préférences obtenues par chaque candidat.
    IDictionary<string, int> CountFirstChoices(List<string> firstChoices);

    // Trouve le ou les candidats ayant obtenu le moins de premières préférences.
    string[] GetCandidatesWithLeastVotes(IDictionary<string, int> firstChoiceCounts);
}
