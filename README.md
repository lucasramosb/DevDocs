<div align="center">
  <h1>DevDocs</h1>
  <p><b>Plataforma inteligente de documentação automatizada para repositórios GitHub, alimentada por IA Local.</b></p>
</div>

---

O **DevDocs** é uma plataforma de engenharia reversa e documentação técnica automatizada, projetada para solucionar a falta de clareza e de especificações técnicas em repositórios de código legados, monolíticos ou complexos.

O objetivo do projeto é mapear a estrutura de repositórios GitHub públicos, analisar a lógica de negócio contida no código-fonte e gerar uma documentação legível e centralizada. Para garantir privacidade e evitar custos recorrentes com APIs de terceiros, toda a pipeline de análise é executada através de **modelos de IA locais (Ollama)**.

## ✨ Funcionalidades Principais

- 🔍 **Mapeamento Estrutural Profundo**: O sistema escaneia a árvore do repositório identificando os arquivos cruciais, dependências e padrões arquiteturais.
- 🤖 **Análise de Contexto com IA Local**: Cada arquivo é analisado por um LLM rodando na sua própria máquina, garantindo que nenhum código proprietário seja vazado para nuvens públicas.
- 📚 **Síntese de Documentação (Markdown)**: Emite um dossiê técnico rico detalhando: Visão Geral, Arquitetura, Tecnologias Utilizadas e os Fluxos Principais (Main Flows).
- ⚡ **Processamento Assíncrono Escalonável**: O back-end utiliza mensageria (Redis) para orquestrar o download e a leitura de grandes volumes de arquivos sem bloquear o front-end.
- 🎨 **Interface de Monitoramento em Tempo Real**: Uma UI moderna (Glassmorphism) interroga a API a cada poucos segundos para montar um "radar" de progresso, acompanhando cada etapa do worker.

---

## 🏗️ Arquitetura

A aplicação foi estruturada utilizando padrões modernos de desenvolvimento em um monorepo, separando responsabilidades para permitir a escalabilidade:

- **Frontend (`app/frontend`)**: Interface web desenvolvida em Next.js 15 e React, utilizando Tailwind CSS e Framer Motion.
- **Backend API (`app/backend/src/DevDocs.Api`)**: Desenvolvida em .NET 10 com Entity Framework Core (SQLite). Atua como o ponto de entrada da aplicação, gerenciando o estado dos projetos e o enfileiramento dos jobs.
- **Backend Worker (`app/backend/src/DevDocs.Worker`)**: Background Service implementado em .NET 10. Consome a fila assincronamente, gerencia downloads do GitHub e interage diretamente com o motor LLM.
- **Mensageria**: Redis, utilizado para comunicação confiável e distribuição da carga entre API e Worker.
- **Motor de IA Local**: Integração com [Ollama](https://ollama.com/) (otimizado e testado utilizando o modelo `qwen2.5-coder:1.5b`).

---

## ⚙️ Setup e Execução

### Pré-requisitos
- **.NET 10 SDK**
- **Node.js** (v18+) e **npm**
- **Docker** (para a instância do Redis)
- **Ollama** em execução local (`ollama run qwen2.5-coder:1.5b`)

### 1. Token de Acesso do GitHub
Para prevenir bloqueios de Rate Limit ao consumir a API pública do GitHub, o sistema requer um Personal Access Token (PAT).

Crie um arquivo chamado `appsettings.Local.json` na raiz da API (`app/backend/src/DevDocs.Api/`) e do Worker (`app/backend/src/DevDocs.Worker/`):
```json
{
  "GitHub": {
    "Token": "ghp_SEU_TOKEN_AQUI"
  }
}
```
*(Nota: O arquivo `appsettings.Local.json` já é ignorado pelo `.gitignore` para prevenir o vazamento de credenciais).*

### 2. Infraestrutura
Na raiz do repositório, inicialize o contêiner do Redis:
```bash
docker-compose up -d
```

### 3. Backend e Processamento
Em um terminal, inicie a API:
```bash
cd app/backend/src/DevDocs.Api
dotnet run
```
Em outro terminal, inicie o Worker responsável pelo processamento de IA:
```bash
cd app/backend/src/DevDocs.Worker
dotnet run
```

### 4. Frontend
Para interagir com a plataforma, inicialize a aplicação client:
```bash
cd app/frontend
npm install
npm run dev
```
Acesse `http://localhost:3000` em seu navegador.

---
**Desenvolvido por Lucas Ramos**
