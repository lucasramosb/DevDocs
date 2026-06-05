import type {
  FileDocumentation,
  IndexingJob,
  Project,
  QueueIndexingJob,
  SourceFile,
  SourceFileContent
} from "@/lib/types";

const projectId = "7f3f1e45-55dd-4e99-bc72-8a7f85d10601";
const apiFileId = "18f82ca5-4700-4596-9ca8-00b5a86bc2e1";
const domainFileId = "f13c1d3b-6f85-4801-99c2-1a8fb7a511d7";
const workerFileId = "7fef6f5a-7602-44b0-a56b-7bb0bf8afded";
const uiFileId = "dbd11ac8-20cf-41f4-9de3-78979241fe54";

export const mockProjects: Project[] = [
  {
    id: projectId,
    name: "DevDocs",
    owner: "open-labs",
    repositoryName: "devdocs",
    gitHubUrl: "https://github.com/open-labs/devdocs",
    defaultBranch: "main",
    description:
      "Motor para indexar repositórios do GitHub e gerar documentação técnica por arquivo.",
    createAt: new Date("2026-06-05T12:10:00.000Z").toISOString()
  }
];

export const mockFiles: SourceFile[] = [
  {
    id: apiFileId,
    projectId,
    path: "backend/src/DevDocs.Api/Endpoints/ProjectsEndpoints.cs",
    name: "ProjectsEndpoints.cs",
    extension: ".cs",
    size: 13942,
    isDocumentationFile: false,
    isTestFile: false,
    createdAt: new Date("2026-06-05T12:16:00.000Z").toISOString()
  },
  {
    id: domainFileId,
    projectId,
    path: "backend/src/DevDocs.Domain/Projects/Project.cs",
    name: "Project.cs",
    extension: ".cs",
    size: 1417,
    isDocumentationFile: false,
    isTestFile: false,
    createdAt: new Date("2026-06-05T12:16:12.000Z").toISOString()
  },
  {
    id: workerFileId,
    projectId,
    path: "backend/src/DevDocs.Worker/ProjectFileMappingWorker.cs",
    name: "ProjectFileMappingWorker.cs",
    extension: ".cs",
    size: 7260,
    isDocumentationFile: false,
    isTestFile: false,
    createdAt: new Date("2026-06-05T12:16:24.000Z").toISOString()
  },
  {
    id: uiFileId,
    projectId,
    path: "README.md",
    name: "README.md",
    extension: ".md",
    size: 2468,
    isDocumentationFile: true,
    isTestFile: false,
    createdAt: new Date("2026-06-05T12:16:36.000Z").toISOString()
  }
];

export const mockJobs: IndexingJob[] = [
  {
    id: "40dd60e8-6255-4651-b826-5b533691fd90",
    projectId,
    status: "Completed",
    totalFilesFound: 246,
    totalFilesMapped: 218,
    totalFilesIgnored: 28,
    errorMessage: null,
    createdAt: new Date("2026-06-05T12:12:00.000Z").toISOString(),
    startedAt: new Date("2026-06-05T12:12:04.000Z").toISOString(),
    finishedAt: new Date("2026-06-05T12:13:18.000Z").toISOString()
  }
];

const contentByFile: Record<string, SourceFileContent> = {
  [apiFileId]: {
    sourceFileId: apiFileId,
    projectId,
    path: "backend/src/DevDocs.Api/Endpoints/ProjectsEndpoints.cs",
    extension: ".cs",
    content: `group.MapPost("/", async (CreateProjectRequest request, IGitHubRepositoryClient gitHubRepositoryClient) =>
{
    if (!GitHubRepositoryUrl.TryParse(request.GitHubUrl, out var repositoryUrl))
    {
        return Results.BadRequest("Informe uma URL válida de repositório do GitHub.");
    }

    var repositoryInfo = await gitHubRepositoryClient.GetPublicRepositoryAsync(
        repositoryUrl.Owner,
        repositoryUrl.RepositoryName,
        cancellationToken
    );

    return Results.Created($"/projects/{project.Id}", response);
});`
  },
  [domainFileId]: {
    sourceFileId: domainFileId,
    projectId,
    path: "backend/src/DevDocs.Domain/Projects/Project.cs",
    extension: ".cs",
    content: `public class Project
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string RepositoryName { get; private set; } = string.Empty;
    public string GitHubUrl { get; private set; } = string.Empty;
    public string DefaultBranch { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
}`
  },
  [workerFileId]: {
    sourceFileId: workerFileId,
    projectId,
    path: "backend/src/DevDocs.Worker/ProjectFileMappingWorker.cs",
    extension: ".cs",
    content: `protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var message = await queue.DequeueAsync(stoppingToken);
        await MapProjectFilesAsync(message.ProjectId, message.IndexingJobId, stoppingToken);
    }
}`
  },
  [uiFileId]: {
    sourceFileId: uiFileId,
    projectId,
    path: "README.md",
    extension: ".md",
    content: `# DevDocs

DevDocs analisa repositórios públicos do GitHub, mapeia arquivos relevantes e gera documentação técnica por arquivo.`
  }
};

const documentationByFile: Record<string, FileDocumentation> = {
  [apiFileId]: {
    id: "6b3608ab-292d-41ff-9a3f-f695b8fb858c",
    projectId,
    sourceFileId: apiFileId,
    summary:
      "Define os endpoints HTTP do recurso Projects e coordena criação, mapeamento, jobs, leitura de arquivos e documentação.",
    content: `## Responsabilidades

- Valida URLs públicas do GitHub antes de persistir um projeto.
- Expõe listagem, detalhe e operações de mapeamento de arquivos.
- Protege leitura e documentação contra arquivos acima de 200 KB.
- Garante que arquivos e jobs pertençam ao projeto informado.

## Endpoints

- POST /projects
- GET /projects
- GET /projects/{id}
- POST /projects/{id}/map-files
- GET /projects/{id}/files
- GET /projects/{projectId}/files/{sourceFileId}/content
- GET /projects/{id}/indexing-jobs
- POST /projects/{projectId}/files/{sourceFileId}/documentation`,
    generator: "mock-docgen",
    createdAt: new Date("2026-06-05T12:18:00.000Z").toISOString()
  }
};

export function createMockProject(gitHubUrl: string): Project {
  const normalized = gitHubUrl.replace(/\/$/, "");
  const parts = normalized.split("/");
  const repositoryName = parts.at(-1) || "repository";
  const owner = parts.at(-2) || "github";

  return {
    id: crypto.randomUUID(),
    name: repositoryName,
    owner,
    repositoryName,
    gitHubUrl,
    defaultBranch: "main",
    description: "Projeto mockado enquanto a API do backend não responde.",
    createAt: new Date().toISOString()
  };
}

export function getMockFiles(selectedProjectId: string): SourceFile[] {
  return mockFiles.map((file) => ({ ...file, projectId: selectedProjectId }));
}

export function getMockJobs(selectedProjectId: string): IndexingJob[] {
  return mockJobs.map((job) => ({ ...job, projectId: selectedProjectId }));
}

export function createMockIndexingJob(selectedProjectId: string): QueueIndexingJob {
  return {
    projectId: selectedProjectId,
    indexingJobId: crypto.randomUUID(),
    status: "Pending",
    message: "Mapeamento de arquivos enviado para processamento."
  };
}

export function getMockFileContent(
  selectedProjectId: string,
  sourceFileId: string
): SourceFileContent {
  const content = contentByFile[sourceFileId] ?? contentByFile[apiFileId];

  return {
    ...content,
    projectId: selectedProjectId,
    sourceFileId
  };
}

export function getMockDocumentation(
  selectedProjectId: string,
  sourceFileId: string
): FileDocumentation {
  const sourceFile = mockFiles.find((file) => file.id === sourceFileId);
  const documentation = documentationByFile[sourceFileId];

  if (documentation) {
    return { ...documentation, projectId: selectedProjectId, sourceFileId };
  }

  return {
    id: crypto.randomUUID(),
    projectId: selectedProjectId,
    sourceFileId,
    summary: `Documentação técnica gerada para ${sourceFile?.name ?? "arquivo selecionado"}.`,
    content: `## Visão geral

Este arquivo participa do fluxo de análise do repositório e deve ser documentado com foco em contrato, responsabilidade e dependências diretas.

## Pontos de atenção

- Validar entradas antes de chamar serviços externos.
- Manter tipos de resposta próximos aos DTOs do backend.
- Registrar erros de processamento para auditoria do job.`,
    generator: "mock-docgen",
    createdAt: new Date().toISOString()
  };
}
