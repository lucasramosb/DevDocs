import {
  createMockIndexingJob,
  createMockProject,
  getMockDocumentation,
  getMockFileContent,
  getMockFiles,
  getMockJobs,
  mockProjects
} from "@/lib/mock-data";
import type {
  ApiResult,
  FileDocumentation,
  IndexingJob,
  Project,
  QueueIndexingJob,
  SourceFile,
  SourceFileContent
} from "@/lib/types";

const basePath = "/api/devdocs";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${basePath}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    }
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `HTTP ${response.status}`);
  }

  return response.json() as Promise<T>;
}

async function withMock<T>(
  apiCall: () => Promise<T>,
  mockCall: () => T | Promise<T>
): Promise<ApiResult<T>> {
  try {
    return {
      data: await apiCall(),
      source: "api"
    };
  } catch {
    return {
      data: await mockCall(),
      source: "mock"
    };
  }
}

export function listProjects() {
  return withMock<Project[]>(() => request<Project[]>("/projects"), () => mockProjects);
}

export function createProject(gitHubUrl: string) {
  return withMock<Project>(
    () =>
      request<Project>("/projects", {
        method: "POST",
        body: JSON.stringify({ gitHubUrl })
      }),
    () => createMockProject(gitHubUrl)
  );
}

export function mapProjectFiles(projectId: string) {
  return withMock<QueueIndexingJob>(
    () =>
      request<QueueIndexingJob>(`/projects/${projectId}/map-files`, {
        method: "POST"
      }),
    () => createMockIndexingJob(projectId)
  );
}

export function listProjectFiles(projectId: string) {
  return withMock<SourceFile[]>(
    () => request<SourceFile[]>(`/projects/${projectId}/files`),
    () => getMockFiles(projectId)
  );
}

export function listIndexingJobs(projectId: string) {
  return withMock<IndexingJob[]>(
    () => request<IndexingJob[]>(`/projects/${projectId}/indexing-jobs`),
    () => getMockJobs(projectId)
  );
}

export function getFileContent(projectId: string, sourceFileId: string) {
  return withMock<SourceFileContent>(
    () =>
      request<SourceFileContent>(
        `/projects/${projectId}/files/${sourceFileId}/content`
      ),
    () => getMockFileContent(projectId, sourceFileId)
  );
}

export function getFileDocumentation(projectId: string, sourceFileId: string) {
  return withMock<FileDocumentation>(
    () =>
      request<FileDocumentation>(
        `/projects/${projectId}/files/${sourceFileId}/documentation`
      ),
    () => getMockDocumentation(projectId, sourceFileId)
  );
}

export function generateFileDocumentation(projectId: string, sourceFileId: string) {
  return withMock<FileDocumentation>(
    () =>
      request<FileDocumentation>(
        `/projects/${projectId}/files/${sourceFileId}/documentation`,
        {
          method: "POST"
        }
      ),
    () => getMockDocumentation(projectId, sourceFileId)
  );
}
