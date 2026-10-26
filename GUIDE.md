# Vote préférentiel – Guide étape par étape

Solution .NET (cible `net8.0`, compile avec le SDK .NET 8, 9 ou 10).

```
VotePreferentiel/
├── VotePreferentiel.sln
├── global.json                      (pour `dotnet test` avec le SDK .NET 10)
├── votes.json                       (exemple du sujet, à pousser sur GitHub)
├── VotePreferentiel/                (3.1 bibliothèque de classes)
│   ├── VotePreferentiel.csproj
│   ├── Models/VoteModel.cs          (3.2)
│   └── Services/
│       ├── IVoteProvider.cs         (3.3)
│       ├── VoteProvider.cs          (3.3)
│       ├── IVoteCounter.cs          (3.5)
│       └── VoteCounter.cs           (3.5)
├── VotePreferentiel.Worker/         (3.4 Worker Service)
│   ├── VotePreferentiel.Worker.csproj
│   ├── Program.cs
│   ├── VoteWorker.cs
│   └── appsettings.json             <- URL du fichier JSON à mettre ici
└── VotePreferentiel.Tests/          (3.6 tests TUnit)
    ├── VotePreferentiel.Tests.csproj
    ├── GetWinnerTests.cs            (les 5 tests demandés)
    ├── VoteCounterMethodsTests.cs   (bonus)
    ├── VoteProviderTests.cs         (bonus)
    └── TestData.cs
```

Vérifier le SDK installé : `dotnet --version`

---

## Étape 0 – Dépôt GitHub

```bash
cd VotePreferentiel
git init
git add .
git commit -m "Initialisation"
git branch -M main
git remote add origin https://github.com/VOTRE-COMPTE/VotePreferentiel.git
git push -u origin main
```

Conseil : faites un commit après chaque étape ci-dessous.

---

## Étape 3.1 – Bibliothèque de classes

**Fichiers :** `VotePreferentiel.sln`, `VotePreferentiel/VotePreferentiel.csproj`

(Commandes utilisées si vous voulez la recréer à la main :
`dotnet new sln -n VotePreferentiel` puis
`dotnet new classlib -n VotePreferentiel -f net8.0` puis
`dotnet sln add VotePreferentiel/VotePreferentiel.csproj`.)

**Tester :**
```bash
dotnet build VotePreferentiel/VotePreferentiel.csproj
```
Attendu : `Build succeeded` / `Générer a réussi`.

---

## Étape 3.2 – Modèle de données

**Fichier :** `VotePreferentiel/Models/VoteModel.cs`

Format JSON prévu :
```json
{ "votes": [ { "voter": "Votant 1", "choices": ["Alice", "Bilal", "Chloé"] } ] }
```

⚠️ **Ouvrez le fichier JSON fourni par le professeur** et comparez les noms de
propriétés. S'ils diffèrent (ex. `"ballots"`, `"preferences"`), changez simplement
les `[JsonPropertyName("...")]` dans `VoteModel.cs`.
(Le `VoteProvider` accepte aussi un tableau de bulletins ou un tableau de tableaux.)

**Tester :** `dotnet build VotePreferentiel/VotePreferentiel.csproj`

---

## Étape 3.3 – Récupération des votes

**Fichiers :** `Services/IVoteProvider.cs`, `Services/VoteProvider.cs`

`VoteProvider` reçoit un `HttpClient` dans son constructeur (injection de
dépendances), fait le `GET`, lit le JSON et le désérialise en `VoteModel`.

**Tester (sans Internet) :**
```bash
dotnet run --project VotePreferentiel.Tests -- --treenode-filter "/*/*/VoteProviderTests/*"
```
Attendu : 2 tests réussis.

---

## Étape 3.4 – Worker Service

**Fichiers :** `VotePreferentiel.Worker/*`

(Création à la main : `dotnet new worker -n VotePreferentiel.Worker -f net8.0`,
`dotnet sln add ...`, `dotnet add VotePreferentiel.Worker reference VotePreferentiel`,
`dotnet add VotePreferentiel.Worker package Microsoft.Extensions.Http`.)

Dans `Program.cs` :
```csharp
builder.Services.AddHttpClient<IVoteProvider, VoteProvider>();
```

1. Mettez l'URL **raw** du fichier JSON dans `VotePreferentiel.Worker/appsettings.json` :
   - soit l'URL donnée par le professeur,
   - soit celle du `votes.json` de votre dépôt :
     `https://raw.githubusercontent.com/VOTRE-COMPTE/VotePreferentiel/main/votes.json`
     (sur GitHub : ouvrir le fichier → bouton **Raw** → copier l'adresse).

2. **Tester :**
```bash
dotnet run --project VotePreferentiel.Worker
```
Attendu (avec `votes.json` de l'exemple) :
```
5 bulletin(s) récupéré(s) :
  Votant 1   : Alice > Bilal > Chloé
  Votant 2   : Alice > Chloé > Bilal
  Votant 3   : Bilal > Chloé > Alice
  Votant 4   : Chloé > Bilal > Alice
  Votant 5   : Chloé > Alice > Bilal
```
Si les choix s'affichent vides → les noms de propriétés JSON ne correspondent pas (voir 3.2).

---

## Étape 3.5 – Calcul du gagnant

**Fichiers :** `Services/IVoteCounter.cs`, `Services/VoteCounter.cs`

| Méthode | Rôle |
|---|---|
| `GetCandidates` | tous les noms présents dans les bulletins |
| `GetAllFirstChoices` | 1er choix de chaque bulletin |
| `CountFirstChoices` | nombre de 1ers choix par candidat |
| `GetCandidatesWithLeastVotes` | le(s) candidat(s) au minimum (égalité → tous) |
| `GetWinner` | boucle : à chaque tour, on retire des bulletins les candidats éliminés (= report des voix), on compte les 1ers choix, on élimine le(s) dernier(s), jusqu'à ce qu'il en reste un |

Cas particuliers : aucun choix exprimé → `InvalidOperationException` ;
égalité parfaite entre tous les candidats restants → `InvalidOperationException`.

Dans `Program.cs` : `builder.Services.AddSingleton<IVoteCounter, VoteCounter>();`
et `VoteWorker` reçoit `IVoteCounter` dans son constructeur.

**Tester :**
```bash
dotnet run --project VotePreferentiel.Worker
```
Attendu en fin de sortie :
```
Candidats : Alice, Bilal, Chloé
Premier tour : Alice = 2, Bilal = 1, Chloé = 2

Gagnant(e) de l'élection : Chloé
```
Et les tests unitaires des méthodes intermédiaires :
```bash
dotnet run --project VotePreferentiel.Tests -- --treenode-filter "/*/*/VoteCounterMethodsTests/*"
```

---

## Étape 3.6 – Tests TUnit

**Fichiers :** `VotePreferentiel.Tests/*` (projet `VotePreferentiel.Tests.csproj`, package `TUnit`)

(Création à la main : `dotnet new install TUnit.Templates` puis
`dotnet new TUnit -n VotePreferentiel.Tests`, ou un projet console avec
`dotnet add package TUnit`.)

`GetWinnerTests.cs` contient les 5 tests demandés :

1. Cas nominal → « Chloé »
2. Un seul candidat → « Alice »
3. Report des préférences → « Chloé »
4. Égalité en dernière position → « Chloé »
5. Bulletin vide → exception

**Lancer tous les tests :**
```bash
dotnet run --project VotePreferentiel.Tests
```
ou
```bash
dotnet test
```
Attendu : `total: 12, failed: 0, succeeded: 12` (5 tests du sujet + 7 bonus).

Seulement les 5 tests du sujet :
```bash
dotnet run --project VotePreferentiel.Tests -- --treenode-filter "/*/*/GetWinnerTests/*"
```

Dans Visual Studio / Rider : ouvrir `VotePreferentiel.sln`, puis l'Explorateur de tests.

---

## Rendu

```bash
git add .
git commit -m "Vote préférentiel : worker, comptage et tests"
git push
git rev-parse HEAD      # <- SHA-1 du dernier commit à mettre dans l'e-mail
```
