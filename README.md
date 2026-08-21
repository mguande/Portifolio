# Portfólio

Site público de portfólio (ASP.NET Core 9) com área autenticada para editar estúdio, textos, perfis, projetos e stacks.

- Site: `/`
- Edição: `/edit` (login em `/edit/login`)

Repositório: [github.com/mguande/Portifolio](https://github.com/mguande/Portifolio)

## Destaques

### Importação de CV do LinkedIn

O caminho mais rápido para montar um perfil no site:

1. No LinkedIn, gere o PDF em **Mais → Salvar em PDF**.
2. Em `/edit/perfis`, importe o arquivo (perfil novo ou um já existente).
3. O parser extrai nome, cargo, resumo, competências, experiências, formação e redes.
4. Aparece uma **prévia** para conferir os dados.
5. Confirme para criar ou atualizar o perfil. Se o perfil já existir, o sistema pergunta antes de sobrescrever e pode **manter a foto** atual.
6. Depois da importação, **edite tudo no formulário**: textos ricos, foto, competências, experiências, formação e redes sociais.

O PDF é o atalho; o cadastro manual continua disponível para quem não usa LinkedIn.

### O que mais o projeto faz

- **CMS próprio** — estúdio, textos do site, perfis, projetos e stacks, sem editar HTML estático.
- **Dois (ou mais) perfis** no mesmo site, com filtro de projetos por pessoa e por trabalhos **conjuntos**.
- **Projetos** com autores, stack, resumo, resultado e ícones de **repositório** ou **link** (site, demo), no mesmo estilo das redes do perfil.
- **Cadastro de stacks** reutilizado na seção Ferramentas & tecnologias e nas opções de cada projeto; um nome novo digitado no projeto entra no cadastro e no vínculo.
- **Editor rico** (TinyMCE) em textos longos; o site público renderiza o HTML.
- **Identidade** (login, usuários, cookie) só na área `/edit`.
- **Banco flexível**: SQLite no dia a dia; PostgreSQL ou MySQL em produção.
- **Hospedagem em subpasta** (`PathBase`), por exemplo `https://servidor/Portifolio` no IIS.

## Ferramentas necessárias

| Ferramenta | Uso |
| --- | --- |
| [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | Compilar e executar |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) 17.12+ (workload **ASP.NET e desenvolvimento web**) **ou** [VS Code](https://code.visualstudio.com/) + C# Dev Kit | Desenvolvimento |
| Navegador atual | Site e área `/edit` |

Opcional:

- [Git](https://git-scm.com/)
- PostgreSQL 15+ ou MySQL 8+, se não quiser SQLite
- No Windows, [ASP.NET Core 9 Hosting Bundle](https://dotnet.microsoft.com/permalink/dotnetcore-current) para publicar no IIS

## Como executar em desenvolvimento

```bash
git clone https://github.com/mguande/Portifolio.git
cd Portifolio
```

**Visual Studio:** abra `Portifolio.sln`, defina **Portifolio.Web** como projeto de inicialização e rode o perfil **http** (ou **https**).

**Linha de comando:**

```bash
dotnet run --project src/Portifolio.Web --launch-profile http
```

O site abre em [http://localhost:5209](http://localhost:5209). A edição fica em [http://localhost:5209/edit](http://localhost:5209/edit).

Na primeira execução o app cria o banco SQLite (se ainda não existir), as tabelas e o usuário administrador. Também importa o conteúdo inicial a partir de `src/Portifolio.Web/Seed/content.js`.

## Configuração

Arquivo principal: `src/Portifolio.Web/appsettings.json`.

Em produção, prefira variáveis de ambiente ou `appsettings.Production.json` (não commite senhas reais).

### Usuário inicial

```json
"Seed": {
  "AdminUserName": "admin",
  "AdminPassword": "Admin@123",
  "AdminEmail": "admin@local"
}
```

O seed **só roda se ainda não houver usuários**. A senha precisa atender às regras do Identity (mínimo 8 caracteres, letra maiúscula, minúscula e número). Troque esses valores **antes da primeira execução** e altere a senha depois do primeiro login.

### Banco de dados

```json
"Database": {
  "Provider": "Sqlite",
  "SqlitePath": "db/portfolio.db",
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Database=portfolio;Username=postgres;Password=postgres",
    "MySql": "Server=localhost;Database=portfolio;User=root;Password=;"
  }
}
```

- `Provider`: `Sqlite` (padrão), `PostgreSql` ou `MySql`
- SQLite: `SqlitePath` é relativo à pasta que contém `Portifolio.sln` (ou absoluto). O arquivo `db/portfolio.db` não entra no Git; cada ambiente gera o seu.

Em produção com vários processos ou backups mais simples, use PostgreSQL (ou MySQL) em vez de SQLite.

### Caminho da aplicação (`PathBase`)

Se o site não estiver na raiz do domínio (exemplo: `https://servidor/Portifolio`):

```json
"PathBase": "/Portifolio"
```

Deixe `""` quando a aplicação estiver em `https://seusite.com/`. O cookie de login e os links `~/` respeitam esse prefixo.

## Uso da área de edição

1. Acesse `/edit/login` (ou `/Portifolio/edit/login` com PathBase).
2. Entre com o usuário seed (ou outro criado em **Usuários**).
3. Edite **Estúdio**, **Textos**, **Perfis**, **Projetos** e **Stacks**.
4. Em **Perfis**, escolha cadastro manual ou o fluxo de importação do LinkedIn descrito em **Destaques**.
5. Depois de importar, abra o perfil e ajuste o que quiser antes (ou depois) de publicar no site.

## Ambientes sugeridos para deploy

### 1. IIS no Windows (já usado neste projeto)

Adequado para um servidor Windows que você já administra.

1. Instale o **Hosting Bundle 9**.
2. Publique em pasta:

   ```bash
   dotnet publish src/Portifolio.Web -c Release -o ./publish
   ```

3. No IIS: site ou **aplicação** sob um site existente. Se for aplicação `/Portifolio`, defina `PathBase` (e, no `web.config` publicado, `ASPNETCORE_ENVIRONMENT=Production`).
4. Pool: *No Managed Code*, identidade com permissão de escrita na pasta do SQLite (`db/`) ou use PostgreSQL.
5. HTTPS com certificado (Let’s Encrypt, ou o certificado do servidor).

O perfil `FolderProfile.pubxml` aponta para `_deploy/web` (pasta ignorada pelo Git). Ajuste o destino na sua máquina.

### 2. Linux + Nginx (ou Caddy) + systemd

Bom custo e operação em VPS (DigitalOcean, Hetzner, Oracle Cloud, etc.).

- Serviço `dotnet Portifolio.Web.dll` atrás do proxy
- `PathBase` vazio se o site for o domínio inteiro
- PostgreSQL no mesmo host ou gerenciado
- Nginx termina TLS e encaminha para `http://127.0.0.1:5000`

### 3. Azure App Service ou container

- **App Service** (Windows ou Linux) com .NET 9: simples se você já usa Azure; connection string e `Seed`/`PathBase` nas *Application settings*
- **Container** (Docker → App Service, Azure Container Apps, Fly.io, Railway): isole runtime 9 + volume ou Postgres para o banco
- Evite SQLite em plano com várias instâncias; use um banco único (PostgreSQL)

### 4. O que evitar

- SQLite em disco efêmero (muitos PaaS apagam o sistema de arquivos a cada deploy)
- Expor `/edit` sem HTTPS
- Manter a senha seed padrão em um site público

## Estrutura

```
Portifolio.sln
src/Portifolio.Web/     site MVC, área /edit, Identity, EF Core
db/                     SQLite local (gerado em runtime)
```

## Licença

Uso pessoal / portfólio. Ajuste este trecho se quiser uma licença explícita (MIT, etc.).
