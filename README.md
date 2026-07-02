<div align="center">
  <h1>DevDocs</h1>
  <p><b>Projeto para análise e documentação automática de repositórios GitHub utilizando Inteligência Artificial.</b></p>
</div>

---

O **DevDocs** é um projeto desenvolvido para automatizar a análise e a documentação técnica de projetos de software hospedados no GitHub.

A proposta é permitir que qualquer desenvolvedor informe apenas a URL de um repositório público e obtenha uma documentação completa gerada automaticamente, reduzindo o tempo necessário para compreender sistemas legados, monólitos ou projetos sem documentação.

Durante o processamento, o projeto identifica a estrutura do repositório, seleciona os arquivos relevantes, analisa seu conteúdo utilizando Inteligência Artificial e gera documentações individuais para cada arquivo, além de uma documentação consolidada contendo visão geral, arquitetura, tecnologias utilizadas e principais fluxos do sistema.

Todo o processamento é executado localmente através do **Ollama**, garantindo privacidade dos dados, independência de serviços externos e a possibilidade de utilizar diferentes modelos de IA sem alterar a arquitetura da aplicação.

Além da geração automática de documentação, o DevDocs também tem como objetivo servir como projeto de estudo para tecnologias modernas de desenvolvimento Full Stack, explorando conceitos como **Clean Architecture**, **Entity Framework Core**, **Redis**, **Workers**, **GitHub API**, **processamento assíncrono** e **Integração com Inteligência Artificial**.

## ✨ Funcionalidades

* 📥 **Importação de Repositórios GitHub**: Recebe a URL de um repositório público, valida sua existência e obtém automaticamente suas informações.
* 🔍 **Mapeamento Inteligente de Arquivos**: Percorre toda a estrutura do projeto identificando apenas arquivos relevantes para análise, ignorando diretórios temporários e arquivos desnecessários.
* 🤖 **Análise Automatizada com IA Local**: Cada arquivo é enviado para um modelo de IA executado localmente através do Ollama, responsável por interpretar o código e gerar uma documentação técnica.
* 📄 **Documentação por Arquivo**: Gera documentação individual contendo responsabilidade, funcionamento, dependências e observações importantes de cada arquivo analisado.
* 📚 **Documentação Geral do Projeto**: Consolida todas as análises em uma documentação única contendo visão geral, arquitetura, tecnologias utilizadas, organização do projeto e principais fluxos.
* ⚡ **Processamento Assíncrono**: Utiliza filas com Redis e Workers para executar tarefas pesadas em background, permitindo o processamento de grandes repositórios sem bloquear a API.
* 🔄 **Arquitetura Extensível**: O sistema foi desenvolvido para permitir a adição de novas linguagens, modelos de IA e provedores de repositórios sem grandes alterações na arquitetura.

---

## 🏗️ Arquitetura

O projeto está organizado em um **Monorepo**, separando frontend e backend para facilitar manutenção, escalabilidade e evolução da aplicação.

### Frontend (`app/frontend`)

Aplicação web desenvolvida com:

* Next.js
* React
* TypeScript
* Tailwind CSS

Responsável pela interação do usuário, acompanhamento do processamento e visualização da documentação gerada.

### Backend API (`app/backend/src/DevDocs.Api`)

Desenvolvida em **.NET 10** utilizando **ASP.NET Core Minimal API**.

Responsável por:

* gerenciamento dos projetos;
* validação dos repositórios GitHub;
* persistência dos dados;
* disponibilização da API;
* criação dos Jobs de processamento.

### Backend Worker (`app/backend/src/DevDocs.Worker`)

Serviço executado em background responsável por todo o processamento pesado da aplicação.

Entre suas responsabilidades estão:

* consumir filas Redis;
* acessar a API do GitHub;
* mapear os arquivos do repositório;
* obter o conteúdo dos arquivos;
* enviar os arquivos para a IA;
* gerar e persistir as documentações.

### Banco de Dados

Durante o desenvolvimento é utilizado:

* SQLite

A arquitetura foi preparada para migração futura para:

* PostgreSQL

### Mensageria

O Redis é utilizado para comunicação entre API e Workers, permitindo o processamento assíncrono das análises.

### Inteligência Artificial

A geração da documentação é realizada através do **Ollama**, permitindo utilizar modelos locais como:

* Qwen
* Gemma
* DeepSeek
* Llama

A implementação foi desacoplada para facilitar futuras integrações com outros provedores de IA.

---

## ⚙️ Setup e Execução

### Pré-requisitos

* .NET 10 SDK
* Node.js 18+
* Docker
* Ollama
* Git

### 1. Configurar Token do GitHub

Para evitar limitações de requisições da API pública do GitHub, configure um **Personal Access Token (PAT)**.

Crie um arquivo chamado **`appsettings.Local.json`** em:

```text
app/backend/src/DevDocs.Api/
```

e

```text
app/backend/src/DevDocs.Worker/
```

com o conteúdo:

```json
{
  "GitHub": {
    "Token": "ghp_SEU_TOKEN_AQUI"
  }
}
```

> O arquivo `appsettings.Local.json` é ignorado pelo `.gitignore`, evitando que credenciais sejam enviadas para o repositório.

---

### 2. Iniciar a infraestrutura

Suba o Redis utilizando Docker:

```bash
docker compose up -d
```

---

### 3. Executar a API

```bash
cd app/backend

dotnet run --project src/DevDocs.Api
```

---

### 4. Executar o Worker

Em outro terminal:

```bash
cd app/backend

dotnet run --project src/DevDocs.Worker
```

---

### 5. Executar o Frontend

```bash
cd app/frontend

npm install

npm run dev
```

A aplicação estará disponível em:

```text
http://localhost:3000
```

<div align="center">

**Desenvolvido por Lucas Ramos**

</div>
