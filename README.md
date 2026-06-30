# DevDocs 🧠📖

**DevDocs** é uma plataforma inovadora alimentada por Inteligência Artificial projetada para gerar automaticamente documentações técnicas completas para repositórios do GitHub. 

O sistema analisa a estrutura do código, entende os fluxos de negócio de cada arquivo e elabora uma documentação centralizada e bem formatada, tudo isso executando **modelos de IA locais (Ollama)** para garantir máxima privacidade e zero custo com APIs externas (como OpenAI).

## 🚀 Arquitetura e Tecnologias

O projeto é dividido em um monorepo moderno contendo:

- **Frontend (`app/frontend`)**: Next.js 15, React, Framer Motion, Tailwind CSS (Glassmorphism / UI Cibernética).
- **Backend API (`app/backend/src/DevDocs.Api`)**: .NET 10, Entity Framework Core (SQLite). Gerencia requisições e enfileira jobs.
- **Backend Worker (`app/backend/src/DevDocs.Worker`)**: .NET 10 (Background Service). Consome a fila, baixa arquivos do GitHub e interage com o Ollama para documentar.
- **Fila/Mensageria**: Redis.
- **Motor de Inteligência Artificial**: [Ollama](https://ollama.com/) (Rodando o modelo `qwen2.5-coder:1.5b`).

---

## 🛠️ Pré-requisitos

Antes de iniciar, certifique-se de ter instalado em sua máquina:
- **.NET 10 SDK**
- **Node.js** (v18+) e **npm**
- **Docker** (para rodar o Redis)
- **Ollama**: Faça o download em `ollama.com` e rode o comando: `ollama run qwen2.5-coder:1.5b` (ou altere o modelo nas configurações).

---

## ⚙️ Configuração (Setup)

### 1. Configurando o Token do GitHub
Para evitar o erro de `Rate Limit` (403 Forbidden) da API pública do GitHub, você deve fornecer um **Personal Access Token (PAT)**.
1. Gere um token no GitHub (Developer Settings -> Personal Access Tokens). Não são necessários escopos se for ler apenas repositórios públicos.
2. Adicione o token no arquivo `appsettings.json` tanto da **API** quanto do **Worker** (`app/backend/src/DevDocs.Api/appsettings.json` e `app/backend/src/DevDocs.Worker/appsettings.json`):

```json
  "GitHub": {
    "Token": "ghp_SEU_TOKEN_AQUI"
  }
```

### 2. Subindo as Dependências (Redis)
Na raiz do projeto (onde está o `docker-compose.yml`), inicie o Redis:
```bash
docker-compose up -d
```

### 3. Rodando o Backend (API e Worker)
Em um terminal, inicie a API:
```bash
cd app/backend/src/DevDocs.Api
dotnet run
```
A API estará rodando em `http://localhost:5150`.

Em um segundo terminal, inicie o Worker (responsável por consumir a fila e falar com a IA):
```bash
cd app/backend/src/DevDocs.Worker
dotnet run
```

### 4. Rodando o Frontend
Em um terceiro terminal, inicie a interface de usuário:
```bash
cd app/frontend
npm install
npm run dev
```
O Frontend estará rodando em `http://localhost:3000`.

---

## 🎮 Como Usar

1. Acesse `http://localhost:3000` em seu navegador.
2. Na tela inicial magnética, cole a URL de qualquer repositório público do GitHub (ex: `https://github.com/owner/repo`).
3. Clique em **Iniciar**.
4. Você será levado para a **Tela de Progresso (Radar de Análise)**. Lá, você verá o sistema mapeando os arquivos, baixando o conteúdo e o Worker enviando tudo para o Ollama documentar.
5. Quando o status mudar para *Concluído*, a **Visão da Documentação** abrirá renderizando o conteúdo em um Markdown rico com syntax highlighting, tabelas e explicações da arquitetura.

## ⚠️ Limpeza de Dados (Reset)
Caso enfrente problemas ou queira limpar todo o banco de dados e recomeçar do zero:
- Pare a API e o Worker.
- Delete o arquivo `app/backend/devdocs.db` (SQLite).
- Limpe a fila do redis executando: `docker-compose down -v`.
- Inicie tudo novamente. A migração do banco (no `.NET`) será feita automaticamente pela API.
