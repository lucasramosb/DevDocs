"use client";

import {
  Activity,
  BookOpenText,
  CheckCircle2,
  Cpu,
  Database,
  FileCode2,
  FolderGit2,
  GitBranch,
  Layers3,
  Loader2,
  Network,
  Play,
  Radar,
  ShieldCheck,
  Sparkles,
  TerminalSquare
} from "lucide-react";
import { type ElementType, FormEvent, type ReactNode, useMemo, useState } from "react";

import {
  createProject,
  listIndexingJobs,
  listProjectFiles,
  mapProjectFiles
} from "@/lib/api";
import { getMockFiles, getMockJobs } from "@/lib/mock-data";
import { formatBytes, formatDate } from "@/lib/format";
import type { DataSource, IndexingJob, Project, SourceFile } from "@/lib/types";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";

type Phase = "idle" | "analyzing" | "ready";

const statusLabel: Record<IndexingJob["status"], string> = {
  Pending: "Pendente",
  Running: "Processando",
  Completed: "Concluido",
  Failed: "Falhou"
};

export function DevDocsDashboard() {
  const [githubUrl, setGithubUrl] = useState("");
  const [phase, setPhase] = useState<Phase>("idle");
  const [source, setSource] = useState<DataSource>("api");
  const [project, setProject] = useState<Project | null>(null);
  const [files, setFiles] = useState<SourceFile[]>([]);
  const [jobs, setJobs] = useState<IndexingJob[]>([]);
  const [error, setError] = useState<string | null>(null);

  const documentation = useMemo(
    () => (project ? buildProjectDocumentation(project, files, jobs) : null),
    [files, jobs, project]
  );

  async function handleAnalyze(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedUrl = githubUrl.trim();
    if (!trimmedUrl) {
      return;
    }

    setPhase("analyzing");
    setError(null);

    try {
      const projectResult = await createProject(trimmedUrl);
      setProject(projectResult.data);
      setSource(projectResult.source);
      setGithubUrl("");

      await mapProjectFiles(projectResult.data.id);

      const [filesResult, jobsResult] = await Promise.all([
        listProjectFiles(projectResult.data.id),
        listIndexingJobs(projectResult.data.id)
      ]);

      const resolvedFiles =
        filesResult.data.length > 0
          ? filesResult.data
          : getMockFiles(projectResult.data.id);
      const resolvedJobs =
        jobsResult.data.length > 0 ? jobsResult.data : getMockJobs(projectResult.data.id);

      setFiles(resolvedFiles);
      setJobs(
        [...resolvedJobs].sort(
          (left, right) =>
            new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime()
        )
      );
      setSource(
        projectResult.source === "mock" ||
          filesResult.source === "mock" ||
          jobsResult.source === "mock" ||
          filesResult.data.length === 0 ||
          jobsResult.data.length === 0
          ? "mock"
          : "api"
      );
      setPhase("ready");
    } catch (caughtError) {
      setError(
        caughtError instanceof Error
          ? caughtError.message
          : "Nao foi possivel analisar o repositorio."
      );
      setPhase("idle");
    }
  }

  if (phase === "idle") {
    return (
      <main className="relative grid min-h-screen place-items-center overflow-hidden px-4">
        <TechBackground />
        <section className="relative z-10 w-full max-w-3xl">
          <div className="mb-6 flex items-center justify-center gap-3">
            <div className="grid size-11 place-items-center rounded-lg border border-primary/35 bg-primary/15 text-primary shadow-[0_0_36px_rgba(20,184,166,0.24)]">
              <BookOpenText className="size-5" />
            </div>
            <h1 className="text-2xl font-semibold tracking-normal text-foreground">
              DevDocs
            </h1>
          </div>

          <form
            onSubmit={handleAnalyze}
            className="flex w-full flex-col gap-3 rounded-lg border border-white/10 bg-card/88 p-2 shadow-2xl shadow-black/30 backdrop-blur md:flex-row"
          >
            <div className="relative flex-1">
              <FolderGit2 className="pointer-events-none absolute left-4 top-3.5 size-5 text-primary" />
              <Input
                value={githubUrl}
                onChange={(event) => setGithubUrl(event.target.value)}
                className="h-12 border-transparent bg-background/80 pl-12 font-mono text-sm shadow-none focus-visible:ring-primary"
                placeholder="https://github.com/owner/repository"
                type="url"
              />
            </div>
            <Button className="h-12 px-5" type="submit">
              <Play />
              Analisar
            </Button>
          </form>

          {error ? (
            <div className="mt-4 rounded-md border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive">
              {error}
            </div>
          ) : null}
        </section>
      </main>
    );
  }

  if (phase === "analyzing" || !documentation || !project) {
    return (
      <main className="relative grid min-h-screen place-items-center overflow-hidden px-4">
        <TechBackground />
        <section className="relative z-10 w-full max-w-xl rounded-lg border border-white/10 bg-card/90 p-6 shadow-2xl shadow-black/30">
          <div className="flex items-center gap-3">
            <div className="grid size-10 place-items-center rounded-md bg-primary/15 text-primary">
              <Loader2 className="size-5 animate-spin" />
            </div>
            <div>
              <p className="text-sm font-semibold">Analisando repositorio</p>
              <p className="mt-1 font-mono text-xs text-muted-foreground">
                indexacao / leitura / consolidacao
              </p>
            </div>
          </div>
          <div className="mt-6 grid gap-3">
            <Progress value={64} />
            <div className="grid grid-cols-3 gap-2 text-center">
              <Signal label="repo" active />
              <Signal label="files" active />
              <Signal label="docs" />
            </div>
          </div>
        </section>
      </main>
    );
  }

  return (
    <main className="relative min-h-screen overflow-hidden px-4 py-5 sm:px-6 lg:px-8">
      <TechBackground />
      <div className="relative z-10 mx-auto flex w-full max-w-[1480px] flex-col gap-5">
        <header className="flex flex-col gap-4 border-b border-white/10 pb-4 md:flex-row md:items-center md:justify-between">
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <div className="grid size-9 place-items-center rounded-md border border-primary/30 bg-primary/15 text-primary">
                <BookOpenText className="size-4" />
              </div>
              <h1 className="truncate text-xl font-semibold">{project.name}</h1>
              <Badge variant={source === "mock" ? "warning" : "success"}>
                {source === "mock" ? "Mock" : "API"}
              </Badge>
            </div>
            <p className="mt-2 truncate font-mono text-xs text-muted-foreground">
              {project.owner}/{project.repositoryName} · {project.defaultBranch}
            </p>
          </div>

          <form onSubmit={handleAnalyze} className="flex w-full gap-2 md:max-w-xl">
            <div className="relative flex-1">
              <FolderGit2 className="pointer-events-none absolute left-3 top-2.5 size-4 text-primary" />
              <Input
                value={githubUrl}
                onChange={(event) => setGithubUrl(event.target.value)}
                className="pl-9 font-mono"
                placeholder="https://github.com/owner/repository"
                type="url"
              />
            </div>
            <Button type="submit">
              <Sparkles />
              Gerar
            </Button>
          </form>
        </header>

        <section className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_380px]">
          <article className="min-w-0 rounded-lg border border-white/10 bg-card/92 shadow-2xl shadow-black/20">
            <div className="border-b border-white/10 p-5">
              <div className="mb-4 flex flex-wrap items-center gap-2">
                <Badge variant="info">
                  <Radar className="mr-1 size-3" />
                  Visao geral
                </Badge>
                <Badge variant="outline">{formatDate(documentation.generatedAt)}</Badge>
              </div>
              <h2 className="text-2xl font-semibold tracking-normal">
                Documentacao tecnica do projeto
              </h2>
              <p className="mt-3 max-w-3xl text-sm leading-6 text-muted-foreground">
                {documentation.summary}
              </p>
            </div>

            <div className="grid gap-5 p-5">
              <section className="grid gap-3 md:grid-cols-3">
                <Metric icon={FileCode2} label="Arquivos fonte" value={documentation.sourceFiles} />
                <Metric icon={Database} label="Tamanho lido" value={documentation.totalSize} />
                <Metric icon={ShieldCheck} label="Cobertura teste" value={documentation.testFiles} />
              </section>

              <section className="grid gap-4 lg:grid-cols-[0.95fr_1.05fr]">
                <Panel title="Arquitetura detectada" icon={Layers3}>
                  <div className="grid gap-2">
                    {documentation.layers.map((layer) => (
                      <div
                        key={layer.name}
                        className="flex items-center justify-between rounded-md border border-white/10 bg-background/70 px-3 py-2"
                      >
                        <div className="min-w-0">
                          <p className="truncate text-sm font-medium">{layer.name}</p>
                          <p className="truncate font-mono text-[11px] text-muted-foreground">
                            {layer.path}
                          </p>
                        </div>
                        <Badge variant="secondary">{layer.files}</Badge>
                      </div>
                    ))}
                  </div>
                </Panel>

                <Panel title="Fluxo operacional" icon={Network}>
                  <div className="grid gap-3">
                    {documentation.flow.map((item, index) => (
                      <div key={item.title} className="flex gap-3">
                        <div className="flex flex-col items-center">
                          <div className="grid size-7 place-items-center rounded-md bg-primary/15 font-mono text-xs text-primary">
                            {index + 1}
                          </div>
                          {index < documentation.flow.length - 1 ? (
                            <div className="h-full w-px bg-border" />
                          ) : null}
                        </div>
                        <div className="pb-3">
                          <p className="text-sm font-medium">{item.title}</p>
                          <p className="mt-1 text-xs leading-5 text-muted-foreground">
                            {item.description}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                </Panel>
              </section>

              <Panel title="Documento consolidado" icon={TerminalSquare}>
                <pre className="max-h-[460px] overflow-auto whitespace-pre-wrap rounded-md border border-white/10 bg-[#08111f] p-4 font-mono text-xs leading-6 text-cyan-50">
                  {documentation.content}
                </pre>
              </Panel>
            </div>
          </article>

          <aside className="grid content-start gap-5">
            <Panel title="Indexacao" icon={Activity}>
              <div className="grid gap-4">
                <div>
                  <div className="mb-2 flex items-center justify-between text-xs text-muted-foreground">
                    <span>Mapeamento</span>
                    <span>{documentation.mappedPercent}%</span>
                  </div>
                  <Progress value={documentation.mappedPercent} />
                </div>
                <div className="grid grid-cols-3 gap-2">
                  <SmallStat label="Encontrados" value={documentation.totalFound} />
                  <SmallStat label="Mapeados" value={documentation.totalMapped} />
                  <SmallStat label="Ignorados" value={documentation.totalIgnored} />
                </div>
              </div>
            </Panel>

            <Panel title="Repositorio" icon={GitBranch}>
              <div className="grid gap-3 text-sm">
                <InfoRow label="Owner" value={project.owner} />
                <InfoRow label="Branch" value={project.defaultBranch} />
                <InfoRow label="Criado" value={formatDate(project.createAt)} />
              </div>
            </Panel>

            <Panel title="Sinais tecnicos" icon={Cpu}>
              <div className="grid gap-2">
                {documentation.signals.map((signal) => (
                  <div
                    key={signal}
                    className="flex items-center gap-2 rounded-md border border-white/10 bg-background/70 px-3 py-2 text-sm"
                  >
                    <CheckCircle2 className="size-4 text-primary" />
                    <span>{signal}</span>
                  </div>
                ))}
              </div>
            </Panel>
          </aside>
        </section>
      </div>
    </main>
  );
}

function TechBackground() {
  return (
    <div className="pointer-events-none absolute inset-0">
      <div className="absolute inset-0 bg-[linear-gradient(rgba(20,184,166,0.08)_1px,transparent_1px),linear-gradient(90deg,rgba(20,184,166,0.08)_1px,transparent_1px)] bg-[size:44px_44px]" />
      <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-primary/50 to-transparent" />
      <div className="absolute left-0 top-1/4 h-px w-full bg-gradient-to-r from-primary/35 via-transparent to-amber-400/25" />
    </div>
  );
}

function Signal({ label, active = false }: { label: string; active?: boolean }) {
  return (
    <div
      className={cn(
        "rounded-md border px-3 py-2 font-mono text-xs",
        active
          ? "border-primary/30 bg-primary/10 text-primary"
          : "border-white/10 bg-background/70 text-muted-foreground"
      )}
    >
      {label}
    </div>
  );
}

function Panel({
  children,
  icon: Icon,
  title
}: {
  children: ReactNode;
  icon: ElementType;
  title: string;
}) {
  return (
    <section className="rounded-lg border border-white/10 bg-card/92 p-4 shadow-xl shadow-black/10">
      <div className="mb-4 flex items-center gap-2">
        <Icon className="size-4 text-primary" />
        <h3 className="text-sm font-semibold">{title}</h3>
      </div>
      {children}
    </section>
  );
}

function Metric({
  icon: Icon,
  label,
  value
}: {
  icon: ElementType;
  label: string;
  value: string | number;
}) {
  return (
    <div className="rounded-lg border border-white/10 bg-background/70 p-4">
      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <Icon className="size-4 text-primary" />
        {label}
      </div>
      <p className="mt-3 text-2xl font-semibold">{value}</p>
    </div>
  );
}

function SmallStat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-md border border-white/10 bg-background/70 px-2 py-3 text-center">
      <p className="font-mono text-sm font-semibold">{value}</p>
      <p className="mt-1 truncate text-[11px] text-muted-foreground">{label}</p>
    </div>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-3 border-b border-white/10 pb-2 last:border-b-0 last:pb-0">
      <span className="text-muted-foreground">{label}</span>
      <span className="truncate font-mono text-xs">{value}</span>
    </div>
  );
}

function buildProjectDocumentation(
  project: Project,
  files: SourceFile[],
  jobs: IndexingJob[]
) {
  const sourceFiles = files.filter((file) => !file.isDocumentationFile);
  const testFiles = files.filter((file) => file.isTestFile);
  const totalBytes = files.reduce((total, file) => total + file.size, 0);
  const latestJob = jobs[0];
  const totalFound = latestJob?.totalFilesFound || files.length;
  const totalMapped = latestJob?.totalFilesMapped || files.length;
  const totalIgnored = latestJob?.totalFilesIgnored || 0;
  const mappedPercent = totalFound ? Math.round((totalMapped / totalFound) * 100) : 0;
  const layerMap = new Map<string, { name: string; path: string; files: number }>();

  for (const file of sourceFiles) {
    const [root = "root", second = ""] = file.path.split("/");
    const key = second ? `${root}/${second}` : root;
    const current = layerMap.get(key);

    if (current) {
      current.files += 1;
    } else {
      layerMap.set(key, {
        name: second || root,
        path: key,
        files: 1
      });
    }
  }

  const layers = Array.from(layerMap.values()).slice(0, 5);
  const generatedAt = latestJob?.finishedAt ?? latestJob?.createdAt ?? new Date().toISOString();

  return {
    generatedAt,
    sourceFiles: sourceFiles.length,
    testFiles: testFiles.length,
    totalSize: formatBytes(totalBytes),
    totalFound,
    totalMapped,
    totalIgnored,
    mappedPercent,
    layers,
    signals: [
      "Repositorio publico validado",
      "Arquivos agrupados por responsabilidade",
      "Documentacao consolidada por dominio",
      statusLabel[latestJob?.status ?? "Completed"]
    ],
    flow: [
      {
        title: "Entrada",
        description: "A URL do GitHub define o escopo da analise e identifica owner, repositorio e branch principal."
      },
      {
        title: "Indexacao",
        description: "O backend mapeia arquivos relevantes, ignora ruido operacional e registra o job de processamento."
      },
      {
        title: "Sintese",
        description: "Os arquivos deixam de ser o destino final e passam a alimentar uma documentacao geral do projeto."
      },
      {
        title: "Entrega",
        description: "O resultado esperado e um documento unico com arquitetura, responsabilidades, fluxos e pontos de evolucao."
      }
    ],
    summary:
      `${project.name} foi analisado como um repositorio ${project.defaultBranch} com ${sourceFiles.length} arquivos fonte mapeados. ` +
      "A documentacao deve ser apresentada como uma visao consolidada do projeto, usando os arquivos apenas como evidencias tecnicas.",
    content: `# ${project.name}

## Visao geral
${project.description ?? "Repositorio analisado a partir da URL informada."}

## Estrutura tecnica
${layers.map((layer) => `- ${layer.name}: ${layer.files} arquivos em ${layer.path}`).join("\n")}

## Fluxo de processamento
- Receber URL publica do GitHub.
- Validar owner, repositorio e branch principal.
- Mapear arquivos relevantes do projeto.
- Consolidar uma documentacao unica por contexto tecnico.

## Direcao do produto
O modelo atual ainda gera documentacao por arquivo, mas a interface ja trata esse conteudo como insumo para uma documentacao geral do projeto. O proximo contrato ideal do backend deve retornar um documento consolidado por projeto, com resumo, arquitetura, dominios, fluxos e riscos tecnicos.`
  };
}
