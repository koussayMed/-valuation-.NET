using System.Text;
using VotePreferentiel.Services;
using VotePreferentiel.Worker;

// Pour afficher correctement « Chloé » dans la console Windows.
Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

// 3.3 : VoteProvider reçoit un HttpClient par injection de dépendances.
builder.Services.AddHttpClient<IVoteProvider, VoteProvider>();

// 3.5 : le service de comptage est injecté via son interface.
builder.Services.AddSingleton<IVoteCounter, VoteCounter>();

builder.Services.AddHostedService<VoteWorker>();

var host = builder.Build();
host.Run();
