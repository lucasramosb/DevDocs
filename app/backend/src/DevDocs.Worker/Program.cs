using DevDocs.Infrastructure;
using DevDocs.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<ProjectAnalysisWorker>();

var host = builder.Build();

host.Run();
