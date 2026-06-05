export type Project = {
  id: string;
  name: string;
  owner: string;
  repositoryName: string;
  gitHubUrl: string;
  defaultBranch: string;
  description: string | null;
  createAt: string;
};

export type SourceFile = {
  id: string;
  projectId: string;
  path: string;
  name: string;
  extension: string;
  size: number;
  isDocumentationFile: boolean;
  isTestFile: boolean;
  createdAt: string;
};

export type SourceFileContent = {
  sourceFileId: string;
  projectId: string;
  path: string;
  extension: string;
  content: string;
};

export type IndexingJobStatus = "Pending" | "Running" | "Completed" | "Failed";

export type IndexingJob = {
  id: string;
  projectId: string;
  status: IndexingJobStatus;
  totalFilesFound: number;
  totalFilesMapped: number;
  totalFilesIgnored: number;
  errorMessage: string | null;
  createdAt: string;
  startedAt: string | null;
  finishedAt: string | null;
};

export type QueueIndexingJob = {
  projectId: string;
  indexingJobId: string;
  status: IndexingJobStatus;
  message: string;
};

export type FileDocumentation = {
  id: string;
  projectId: string;
  sourceFileId: string;
  summary: string;
  content: string;
  generator: string;
  createdAt: string;
};

export type DataSource = "api" | "mock";

export type ApiResult<T> = {
  data: T;
  source: DataSource;
};
