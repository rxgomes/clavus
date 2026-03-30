# ErpIxact — Guia de Arquitetura e Implementação

Este documento é o ponto de partida para qualquer novo módulo. Cada módulo terá seu próprio arquivo de documentação (`MODULO_nome.md`) que seguirá a estrutura e convenções definidas aqui.

---

## Visão Geral

**ErpIxact** é um ERP modular construído como um **Monólito Modular** com **Clean Architecture** e **CQRS** via MediatR.

- **Runtime**: .NET 10.0
- **Banco de dados**: PostgreSQL (via Npgsql + EF Core 9)
- **Documentação de API**: Scalar (OpenAPI)
- **Mensageria interna**: MediatR 14

---

## Estrutura de Pastas

```
ErpIxact/
├── Modules/
│   └── <NomeModulo>/
│       ├── <NomeModulo>.Domain
│       ├── <NomeModulo>.Application
│       └── <NomeModulo>.Infrastructure
├── Shared/
│   └── Shared.Kernel
└── WebApp
```

Cada módulo é **autossuficiente**: tem seu próprio contexto de banco de dados, suas migrations e seu registro de DI. Módulos não referenciam outros módulos — apenas o `Shared.Kernel`.

---

## Shared.Kernel

Contém os blocos de construção reutilizáveis por todos os módulos.

| Tipo | Arquivo | Descrição |
|---|---|---|
| Entidade base | `BaseEntity.cs` | `Id` (Guid v7), `CreatedAt`, `UpdatedAt`, `Active`, suporte a Domain Events |
| Resultado | `Result.cs` | `Result<T>` com `IsSuccess`, `Error`, `ErrorType` |
| Evento de domínio | `IDomainEvent.cs` | Interface de marcação para eventos |
| Value Object | `ValueObjects/DocNumber.cs` | CPF/CNPJ validado |

### Result Pattern

Sempre retorne `Result<T>` nas operações de Application e Domain — nunca lance exceções para fluxo de negócio.

```csharp
// Sucesso
return Result<PartnerDto>.Success(dto);

// Falha de validação
return Result<PartnerDto>.Failure("Mensagem de erro", ErrorType.Validation);

// Não encontrado
return Result<PartnerDto>.NotFound("Mensagem");

// Conflito (duplicidade)
return Result<PartnerDto>.Conflict("Mensagem");
```

Os `ErrorType` mapeiam para HTTP no controller:

| ErrorType | HTTP |
|---|---|
| `Validation` | 400 Bad Request |
| `NotFound` | 404 Not Found |
| `Conflict` | 409 Conflict |
| `Unexpected` | 500 Internal Server Error |

---

## Camada Domain

**Projeto**: `<NomeModulo>.Domain`
**Dependência**: `Shared.Kernel`

### Responsabilidades
- Entidades de domínio
- Value Objects específicos do módulo
- Interfaces de repositório (`IXxxRepository`)
- Exceções de domínio (`DomainException`)
- Constantes de mensagens (`XxxMessages`)
- Domain Events (quando necessário)

### Estrutura de pastas

```
<NomeModulo>.Domain/
├── Entities/
├── ValueObjects/
├── Repositories/
├── Events/
├── Exceptions/
│   └── DomainException.cs
└── Messages/
    └── <NomeModulo>Messages.cs
```

### Entidade

```csharp
public class MinhaEntidade : BaseEntity
{
    public string Nome { get; private set; }

    // Construtor público para criação
    public MinhaEntidade(string nome)
    {
        // Validações lançam DomainException ou retornam via Result
        Nome = nome;
    }

    // Construtor protegido exigido pelo EF Core
    protected MinhaEntidade() { }

    public void Atualizar(string nome)
    {
        Nome = nome;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Mensagens

Todas as mensagens voltadas ao usuário ficam centralizadas em `<NomeModulo>Messages.cs`, organizadas em classes estáticas aninhadas:

```csharp
public static class MinhaEntidadeMessages
{
    public static class Errors
    {
        public const string NomeRequired = "O nome é obrigatório.";
        public const string NotFound = "Registro não encontrado.";
        public const string AlreadyExists = "Registro já existe.";
    }

    public static class Success
    {
        public const string Created = "Criado com sucesso.";
        public const string Updated = "Atualizado com sucesso.";
        public const string Deleted = "Excluído com sucesso.";
    }

    public static class Alerts
    {
        // avisos que não impedem a operação
    }
}
```

---

## Camada Application

**Projeto**: `<NomeModulo>.Application`
**Dependências**: `<NomeModulo>.Domain`, `Shared.Kernel`, `MediatR`

### Responsabilidades
- Commands e Queries (CQRS)
- Handlers MediatR
- DTOs
- Registro de DI

### Estrutura de pastas

```
<NomeModulo>.Application/
├── Commands/
│   ├── Create<Entidade>/
│   │   ├── Create<Entidade>Command.cs
│   │   └── Create<Entidade>CommandHandler.cs
│   └── Update<Entidade>/
│       ├── Update<Entidade>Command.cs
│       └── Update<Entidade>CommandHandler.cs
├── Queries/
│   ├── Get<Entidades>/
│   │   ├── Get<Entidades>Query.cs
│   │   └── Get<Entidades>QueryHandler.cs
│   └── Get<Entidade>ById/
│       ├── Get<Entidade>ByIdQuery.cs
│       └── Get<Entidade>ByIdQueryHandler.cs
├── DTOs/
│   └── <Entidade>Dto.cs
└── DependencyInjection.cs
```

### Command

```csharp
// Command é um record que implementa IRequest<Result<T>>
public record Create<Entidade>Command(string Nome) : IRequest<Result<<Entidade>Dto>>;
```

### CommandHandler

```csharp
public class Create<Entidade>CommandHandler(I<Entidade>Repository repository)
    : IRequestHandler<Create<Entidade>Command, Result<<Entidade>Dto>>
{
    public async Task<Result<<Entidade>Dto>> Handle(
        Create<Entidade>Command request, CancellationToken cancellationToken)
    {
        // 1. Verificar duplicidade (se aplicável)
        // 2. Criar entidade
        // 3. Persistir
        // 4. Retornar Result
    }
}
```

### Query

```csharp
public record Get<Entidade>ByIdQuery(Guid Id) : IRequest<Result<<Entidade>Dto>>;
```

### DTO

```csharp
public record <Entidade>Dto(Guid Id, string Nome, bool Active);
```

### DependencyInjection

```csharp
public static class DependencyInjection
{
    public static IServiceCollection Add<NomeModulo>Application(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
```

---

## Camada Infrastructure

**Projeto**: `<NomeModulo>.Infrastructure`
**Dependências**: `<NomeModulo>.Domain`, `Shared.Kernel`, `EF Core`, `Npgsql`

### Responsabilidades
- DbContext do módulo
- Configurações Fluent API do EF Core
- Implementação dos repositórios
- Migrations
- Registro de DI (recebe a connection string)

### Estrutura de pastas

```
<NomeModulo>.Infrastructure/
├── Data/
│   ├── <NomeModulo>DbContext.cs
│   ├── Configurations/
│   │   └── <Entidade>Configuration.cs
│   └── Migrations/
├── Repositories/
│   └── <Entidade>Repository.cs
└── DependencyInjection.cs
```

### DbContext

```csharp
public class <NomeModulo>DbContext(DbContextOptions<<NomeModulo>DbContext> options)
    : DbContext(options)
{
    public DbSet<<Entidade>> <Entidades> { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(<NomeModulo>DbContext).Assembly);
    }
}
```

### DependencyInjection

```csharp
public static class DependencyInjection
{
    public static IServiceCollection Add<NomeModulo>Infrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<<NomeModulo>DbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<I<Entidade>Repository, <Entidade>Repository>();

        return services;
    }
}
```

---

## WebApp (Controller)

**Projeto**: `WebApp`
**Dependências**: `<NomeModulo>.Application`, `<NomeModulo>.Infrastructure`

### Registro no Program.cs

```csharp
builder.Services.Add<NomeModulo>Application();
builder.Services.Add<NomeModulo>Infrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);
```

### Controller

```csharp
[ApiController]
[Route("api/<nome-modulo>")]
public class <NomeModulo>Controller(IMediator mediator) : ControllerBase
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new Get<Entidades>Query());
        return result.IsSuccess ? Ok(result.Value) : result.ToErrorResponse();
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] Create<Entidade>Command command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Created(string.Empty, result.Value) : result.ToErrorResponse();
    }
}
```

---

## Convenções Gerais

### Obrigatórias (definidas no CLAUDE.md)

- Todo `if`, `for`, `while`, `foreach` deve ter chaves `{}`, mesmo com uma única linha de corpo.

### Nomenclatura

| Elemento | Convenção | Exemplo |
|---|---|---|
| Entidade | PascalCase, singular | `Partner`, `Invoice` |
| Repositório | `I<Entidade>Repository` | `IPartnersRepository` |
| Command | `<Verbo><Entidade>Command` | `CreatePartnerCommand` |
| Query | `Get<Entidade><Critério>Query` | `GetPartnerByIdQuery` |
| Handler | `<Command/Query>Handler` | `CreatePartnerCommandHandler` |
| DTO | `<Entidade>Dto` | `PartnerDto` |
| DbContext | `<Módulo>DbContext` | `PartnersDbContext` |
| Messages | `<Entidade>Messages` | `PartnersMessages` |

### Idioma

- **Código** (nomes de classes, métodos, propriedades): inglês
- **Mensagens ao usuário** (constantes em `Messages`): português
- **Comentários e documentação** (arquivos `.md`): português

### Banco de dados

- Nomes de tabelas e colunas: `snake_case` (configurado via Fluent API)
- Cada módulo tem seu próprio `DbContext` e suas próprias migrations
- Nunca compartilhar DbContext entre módulos

### Tratamento de erros

- Fluxo de negócio: usar `Result<T>` — nunca `throw` para casos esperados
- Exceções de domínio (`DomainException`): apenas para violações de invariantes que representam bugs
- O controller é o único lugar que converte `Result` em resposta HTTP

---

## Checklist para novo módulo

Ao criar um novo módulo, verificar:

- [ ] Pasta `Modules/<NomeModulo>/` criada
- [ ] Projetos `.Domain`, `.Application`, `.Infrastructure` criados e referenciados na solution
- [ ] `Shared.Kernel` referenciado em Domain e Application
- [ ] Entidade(s) criada(s) herdando `BaseEntity`
- [ ] Interface(s) de repositório definida(s) em Domain
- [ ] Mensagens centralizadas em `<Entidade>Messages.cs`
- [ ] Commands e Queries implementados com handlers
- [ ] DTOs definidos
- [ ] `DependencyInjection.cs` criado em Application e Infrastructure
- [ ] DbContext criado com `ApplyConfigurationsFromAssembly`
- [ ] Configuração Fluent API criada para cada entidade
- [ ] Repositório implementado
- [ ] Migration gerada
- [ ] Controller criado em WebApp
- [ ] Módulo registrado no `Program.cs`
- [ ] Arquivo `MODULO_<nome>.md` criado com documentação do módulo

---

## Módulos existentes

| Módulo | Documentação | Status |
|---|---|---|
| Partners | [MODULO_Partners.md](./MODULO_Partners.md) | Em produção |
