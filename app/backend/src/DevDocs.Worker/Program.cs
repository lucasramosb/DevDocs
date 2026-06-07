using DevDocs.Infrastructure;
using DevDocs.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<ProjectFileMappingWorker>();

builder.Services.AddHostedService<FileDocumentationGenerationWorker>();

var host = builder.Build();

host.Run();
