
# Enums — Convenção para Campos com Valores Fixos

Adicionar esta seção ao guia de arquitetura, após a seção "Camada Domain".

---

## Enums

# ErpIxact — Guia de Arquitetura e Implementação

Este documento é o ponto de partida para qualquer novo módulo. Cada módulo terá seu próprio arquivo de documentação (`MODULO_nome.md`) que seguirá a estrutura e convenções definidas aqui.

---

## Visão Geral

**ErpIxact** é um ERP modular construído como um **Monólito Modular** com **Clean Architecture** e **CQRS** via MediatR.

- **Runtime**: .NET 10.0
- **Banco de dados**: PostgreSQL (via Npgsql + EF Core 9)
- **Documentação de API**: Scalar (OpenAPI)
- **Mensageria interna**: MediatR 14
- **Testes**: xUnit + FluentAssertions + NSubstitute

---

## Estrutura de Pastas

```
ErpIxact/
├── Modules/
│   └── <NomeModulo>/
│       ├── <NomeModulo>.Domain
│       ├── <NomeModulo>.Application
│       ├── <NomeModulo>.Infrastructure
│       └── <NomeModulo>.Tests
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
| Paginação | `PagedResult.cs` | Resultado paginado genérico |
| Filtro base | `PagedQuery.cs` | Query base com `Page`, `PageSize`, `Search` |

### BaseEntity (Implementação Completa)

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool Active { get; set; } = true;

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Soft delete — marca o registro como inativo.
    /// </summary>
    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reativa um registro previamente desativado.
    /// </summary>
    public void Activate()
    {
        Active = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

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

### PagedResult

```csharp
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

### PagedQuery

```csharp
public abstract record PagedQuery<T>(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<Result<PagedResult<T>>>;
```

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
├── Enums/
│   ├── <Entidade>Status.cs
│   └── <Entidade>StatusExtensions.cs
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
    public MinhaEntidadeStatus Status { get; private set; }

    // Construtor público para criação
    public MinhaEntidade(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException(MinhaEntidadeMessages.Errors.NomeRequired);
        }

        Nome = nome;
        Status = MinhaEntidadeStatus.Active;
    }

    // Construtor protegido exigido pelo EF Core
    protected MinhaEntidade() { }

    public void Atualizar(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException(MinhaEntidadeMessages.Errors.NomeRequired);
        }

        Nome = nome;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### Interface de Repositório

Cada repositório define um contrato mínimo. Não usar repositório genérico — cada entidade tem seu repositório específico com métodos que fazem sentido para ela.

```csharp
public interface IMinhaEntidadeRepository
{
    Task<MinhaEntidade?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<MinhaEntidade> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, string? search = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task AddAsync(MinhaEntidade entidade, CancellationToken cancellationToken = default);
    Task UpdateAsync(MinhaEntidade entidade, CancellationToken cancellationToken = default);
}
```

**Regras de repositório:**
- `GetByIdAsync` retorna apenas registros **ativos** (`Active == true`)
- `GetAllAsync` retorna apenas registros **ativos**, com paginação e busca opcional
- Não existe `DeleteAsync` — o delete é sempre soft delete via `Deactivate()` seguido de `UpdateAsync`
- Métodos de verificação (`ExistsByXxxAsync`) são adicionados conforme necessidade de validação de duplicidade

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
        public const string InvalidStatusTransition = "Transição de status inválida.";
    }

    public static class Success
    {
        public const string Created = "Criado com sucesso.";
        public const string Updated = "Atualizado com sucesso.";
        public const string Deleted = "Excluído com sucesso.";
        public const string Activated = "Reativado com sucesso.";
    }

    public static class Alerts
    {
        // avisos que não impedem a operação
    }
}
```

### Enums

Sempre que uma entidade possuir um campo com conjunto finito de valores (status, tipo, categoria, etc.), utilize **enums simples do C#** com **Extension Methods** para labels amigáveis em português.

#### Onde ficam os Enums

| Escopo | Local | Exemplo |
|---|---|---|
| Específico de um módulo | `<NomeModulo>.Domain/Enums/` | `InvoiceStatus`, `PaymentMethod` |
| Compartilhado entre módulos | `Shared.Kernel/Enums/` | `ApprovalStatus`, `DocumentType` |

> **Regra**: o enum só vai para o `Shared.Kernel` quando **dois ou mais módulos** precisam dele. Caso contrário, ele pertence ao Domain do módulo.

#### Convenção de nomenclatura

| Elemento | Convenção | Exemplo |
|---|---|---|
| Enum | PascalCase, sem prefixo `E` | `InvoiceStatus`, `PaymentMethod` |
| Valores | PascalCase, descritivos, em inglês | `Pending`, `Sold`, `NearExpiry` |
| Extension class | `<NomeEnum>Extensions` | `SaleStatusExtensions` |
| Método de label | `ToLabel()` | `status.ToLabel()` → `"Próximo do Vencimento"` |

#### Definição do Enum

```csharp
namespace ErpIxact.Modules.Sales.Domain.Enums;

/// <summary>
/// Status possíveis de uma venda.
/// </summary>
public enum SaleStatus
{
    Pending = 1,
    Reserved = 2,
    Sold = 3,
    NearExpiry = 4,
    Expired = 5,
    Cancelled = 6
}
```

**Regras:**

- Sempre atribuir valores numéricos explícitos (começando em 1, nunca 0).
- Valor 0 nunca é usado — evita problemas com `default(Enum)` retornando um estado válido acidentalmente.
- Nomes dos valores em **inglês** (segue a convenção do projeto para código).
- Documentar o enum com `<summary>` quando o contexto não for óbvio.

#### Extension Method para Labels Amigáveis

Cada enum tem um arquivo `<NomeEnum>Extensions.cs` no mesmo diretório com o método `ToLabel()`:

```csharp
namespace ErpIxact.Modules.Sales.Domain.Enums;

public static class SaleStatusExtensions
{
    public static string ToLabel(this SaleStatus status) => status switch
    {
        SaleStatus.Pending    => "Pendente",
        SaleStatus.Reserved   => "Reservado",
        SaleStatus.Sold       => "Vendido",
        SaleStatus.NearExpiry => "Próximo do Vencimento",
        SaleStatus.Expired    => "Vencido",
        SaleStatus.Cancelled  => "Cancelado",
        _ => status.ToString()
    };
}
```

**Regras do Extension Method:**

- O nome do método é sempre `ToLabel()`.
- Os labels são em **português** (mensagens voltadas ao usuário).
- O `default (_)` retorna `status.ToString()` como fallback seguro.
- Um arquivo por enum: `SaleStatus.cs` + `SaleStatusExtensions.cs`.

#### Uso na Entidade

```csharp
public class Sale : BaseEntity
{
    public string Description { get; private set; }
    public SaleStatus Status { get; private set; }

    public Sale(string description)
    {
        Description = description;
        Status = SaleStatus.Pending; // estado inicial explícito
    }

    protected Sale() { }

    public void MarkAsSold()
    {
        if (Status != SaleStatus.Reserved)
        {
            throw new DomainException(SaleMessages.Errors.InvalidStatusTransition);
        }

        Status = SaleStatus.Sold;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

#### Configuração no EF Core (Fluent API)

Enums são armazenados como **string** no banco para legibilidade:

```csharp
builder.Property(s => s.Status)
    .HasConversion<string>()
    .HasColumnName("status")
    .HasMaxLength(50)
    .IsRequired();
```

> **Por que string e não int?** Em um ERP, legibilidade no banco é importante para consultas manuais, relatórios e debugging. O custo de performance é desprezível.

#### No DTO

O DTO expõe tanto o valor do enum quanto o label amigável:

```csharp
public record SaleDto(
    Guid Id,
    string Description,
    string Status,         // valor do enum: "NearExpiry"
    string StatusLabel,    // label amigável: "Próximo do Vencimento"
    bool Active
);
```

Conversão no handler:

```csharp
var dto = new SaleDto(
    sale.Id,
    sale.Description,
    sale.Status.ToString(),
    sale.Status.ToLabel(),
    sale.Active
);
```

> O front recebe `Status` para lógica e `StatusLabel` para exibição ao usuário.

### Domain Events

Domain Events são usados para notificar outras partes do sistema sobre algo que aconteceu. Nunca para orquestrar fluxos — apenas para reações.

```csharp
// Definição do evento
public record MinhaEntidadeCriadaEvent(Guid EntidadeId, string Nome) : IDomainEvent;

// Publicação na entidade
public MinhaEntidade(string nome)
{
    Nome = nome;
    AddDomainEvent(new MinhaEntidadeCriadaEvent(Id, nome));
}
```

A publicação dos eventos é feita automaticamente via interceptor do EF Core no `SaveChangesAsync` (veja seção Infrastructure).

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
│   ├── Update<Entidade>/
│   │   ├── Update<Entidade>Command.cs
│   │   └── Update<Entidade>CommandHandler.cs
│   ├── Delete<Entidade>/
│   │   ├── Delete<Entidade>Command.cs
│   │   └── Delete<Entidade>CommandHandler.cs
│   └── Activate<Entidade>/
│       ├── Activate<Entidade>Command.cs
│       └── Activate<Entidade>CommandHandler.cs
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

### Command — Criação

```csharp
public record CreateMinhaEntidadeCommand(string Nome) : IRequest<Result<MinhaEntidadeDto>>;
```

### CommandHandler — Criação

```csharp
public class CreateMinhaEntidadeCommandHandler(IMinhaEntidadeRepository repository)
    : IRequestHandler<CreateMinhaEntidadeCommand, Result<MinhaEntidadeDto>>
{
    public async Task<Result<MinhaEntidadeDto>> Handle(
        CreateMinhaEntidadeCommand request, CancellationToken cancellationToken)
    {
        // 1. Validação de entrada
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return Result<MinhaEntidadeDto>.Failure(
                MinhaEntidadeMessages.Errors.NomeRequired, ErrorType.Validation);
        }

        // 2. Verificar duplicidade (se aplicável)
        var exists = await repository.ExistsByNomeAsync(request.Nome, cancellationToken);
        if (exists)
        {
            return Result<MinhaEntidadeDto>.Conflict(
                MinhaEntidadeMessages.Errors.AlreadyExists);
        }

        // 3. Criar entidade
        var entidade = new MinhaEntidade(request.Nome);

        // 4. Persistir
        await repository.AddAsync(entidade, cancellationToken);

        // 5. Retornar Result com DTO
        var dto = new MinhaEntidadeDto(entidade.Id, entidade.Nome, entidade.Active);
        return Result<MinhaEntidadeDto>.Success(dto);
    }
}
```

### Command — Atualização

```csharp
public record UpdateMinhaEntidadeCommand(Guid Id, string Nome) : IRequest<Result<MinhaEntidadeDto>>;
```

### CommandHandler — Atualização

```csharp
public class UpdateMinhaEntidadeCommandHandler(IMinhaEntidadeRepository repository)
    : IRequestHandler<UpdateMinhaEntidadeCommand, Result<MinhaEntidadeDto>>
{
    public async Task<Result<MinhaEntidadeDto>> Handle(
        UpdateMinhaEntidadeCommand request, CancellationToken cancellationToken)
    {
        // 1. Buscar entidade existente
        var entidade = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entidade is null)
        {
            return Result<MinhaEntidadeDto>.NotFound(
                MinhaEntidadeMessages.Errors.NotFound);
        }

        // 2. Verificar duplicidade (se o nome mudou)
        if (!entidade.Nome.Equals(request.Nome, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await repository.ExistsByNomeAsync(request.Nome, cancellationToken);
            if (exists)
            {
                return Result<MinhaEntidadeDto>.Conflict(
                    MinhaEntidadeMessages.Errors.AlreadyExists);
            }
        }

        // 3. Atualizar entidade
        entidade.Atualizar(request.Nome);

        // 4. Persistir
        await repository.UpdateAsync(entidade, cancellationToken);

        // 5. Retornar Result com DTO
        var dto = new MinhaEntidadeDto(entidade.Id, entidade.Nome, entidade.Active);
        return Result<MinhaEntidadeDto>.Success(dto);
    }
}
```

### Command — Soft Delete

```csharp
public record DeleteMinhaEntidadeCommand(Guid Id) : IRequest<Result<string>>;
```

### CommandHandler — Soft Delete

```csharp
public class DeleteMinhaEntidadeCommandHandler(IMinhaEntidadeRepository repository)
    : IRequestHandler<DeleteMinhaEntidadeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        DeleteMinhaEntidadeCommand request, CancellationToken cancellationToken)
    {
        var entidade = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entidade is null)
        {
            return Result<string>.NotFound(MinhaEntidadeMessages.Errors.NotFound);
        }

        entidade.Deactivate();
        await repository.UpdateAsync(entidade, cancellationToken);

        return Result<string>.Success(MinhaEntidadeMessages.Success.Deleted);
    }
}
```

### Command — Reativação

```csharp
public record ActivateMinhaEntidadeCommand(Guid Id) : IRequest<Result<string>>;
```

### CommandHandler — Reativação

```csharp
public class ActivateMinhaEntidadeCommandHandler(IMinhaEntidadeRepository repository)
    : IRequestHandler<ActivateMinhaEntidadeCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ActivateMinhaEntidadeCommand request, CancellationToken cancellationToken)
    {
        // Nota: para reativação, buscar INCLUINDO inativos
        var entidade = await repository.GetByIdIncludingInactiveAsync(
            request.Id, cancellationToken);
        if (entidade is null)
        {
            return Result<string>.NotFound(MinhaEntidadeMessages.Errors.NotFound);
        }

        entidade.Activate();
        await repository.UpdateAsync(entidade, cancellationToken);

        return Result<string>.Success(MinhaEntidadeMessages.Success.Activated);
    }
}
```

### Query — Listagem Paginada

```csharp
public record GetMinhaEntidadesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<Result<PagedResult<MinhaEntidadeDto>>>;
```

### QueryHandler — Listagem Paginada

```csharp
public class GetMinhaEntidadesQueryHandler(IMinhaEntidadeRepository repository)
    : IRequestHandler<GetMinhaEntidadesQuery, Result<PagedResult<MinhaEntidadeDto>>>
{
    public async Task<Result<PagedResult<MinhaEntidadeDto>>> Handle(
        GetMinhaEntidadesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetAllAsync(
            request.Page, request.PageSize, request.Search, cancellationToken);

        var dtos = items.Select(e => new MinhaEntidadeDto(
            e.Id, e.Nome, e.Active)).ToList();

        var pagedResult = new PagedResult<MinhaEntidadeDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return Result<PagedResult<MinhaEntidadeDto>>.Success(pagedResult);
    }
}
```

### Query — Por ID

```csharp
public record GetMinhaEntidadeByIdQuery(Guid Id) : IRequest<Result<MinhaEntidadeDto>>;
```

### QueryHandler — Por ID

```csharp
public class GetMinhaEntidadeByIdQueryHandler(IMinhaEntidadeRepository repository)
    : IRequestHandler<GetMinhaEntidadeByIdQuery, Result<MinhaEntidadeDto>>
{
    public async Task<Result<MinhaEntidadeDto>> Handle(
        GetMinhaEntidadeByIdQuery request, CancellationToken cancellationToken)
    {
        var entidade = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entidade is null)
        {
            return Result<MinhaEntidadeDto>.NotFound(
                MinhaEntidadeMessages.Errors.NotFound);
        }

        var dto = new MinhaEntidadeDto(entidade.Id, entidade.Nome, entidade.Active);
        return Result<MinhaEntidadeDto>.Success(dto);
    }
}
```

### DTO

```csharp
public record MinhaEntidadeDto(Guid Id, string Nome, bool Active);
```

### DependencyInjection

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddMinhaModuloApplication(
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
public class MinhaModuloDbContext(DbContextOptions<MinhaModuloDbContext> options)
    : DbContext(options)
{
    public DbSet<MinhaEntidade> MinhaEntidades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MinhaModuloDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Publicação automática de Domain Events via MediatR
        var domainEntities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        domainEntities.ForEach(e => e.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        // Publicar eventos APÓS o SaveChanges para garantir consistência
        // (Requer injeção de IMediator ou IPublisher se usar este padrão)

        return result;
    }
}
```

### Configuração Fluent API

```csharp
public class MinhaEntidadeConfiguration : IEntityTypeConfiguration<MinhaEntidade>
{
    public void Configure(EntityTypeBuilder<MinhaEntidade> builder)
    {
        builder.ToTable("minha_entidades");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Active)
            .HasColumnName("active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Ignorar DomainEvents — não persiste no banco
        builder.Ignore(e => e.DomainEvents);

        // Índices
        builder.HasIndex(e => e.Active);
    }
}
```

### Implementação do Repositório

```csharp
public class MinhaEntidadeRepository(MinhaModuloDbContext context)
    : IMinhaEntidadeRepository
{
    public async Task<MinhaEntidade?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MinhaEntidades
            .Where(e => e.Active)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Busca incluindo inativos — usar APENAS para reativação.
    /// </summary>
    public async Task<MinhaEntidade?> GetByIdIncludingInactiveAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MinhaEntidades
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<MinhaEntidade> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.MinhaEntidades
            .Where(e => e.Active)
            .AsQueryable();

        // Filtro de busca textual
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(e => e.Nome.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> ExistsByNomeAsync(
        string nome, CancellationToken cancellationToken = default)
    {
        return await context.MinhaEntidades
            .Where(e => e.Active)
            .AnyAsync(e => e.Nome.ToLower() == nome.ToLower(), cancellationToken);
    }

    public async Task AddAsync(
        MinhaEntidade entidade, CancellationToken cancellationToken = default)
    {
        await context.MinhaEntidades.AddAsync(entidade, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        MinhaEntidade entidade, CancellationToken cancellationToken = default)
    {
        context.MinhaEntidades.Update(entidade);
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

### DependencyInjection

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddMinhaModuloInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<MinhaModuloDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IMinhaEntidadeRepository, MinhaEntidadeRepository>();

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
builder.Services.AddMinhaModuloApplication();
builder.Services.AddMinhaModuloInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);
```

### Padrão de Rotas da API

Todas as rotas seguem o padrão:

| Operação | Verbo HTTP | Rota | Retorno |
|---|---|---|---|
| Listar (paginado) | `GET` | `api/<modulo>/get-all` | `200 Ok` com `PagedResult<Dto>` |
| Buscar por ID | `GET` | `api/<modulo>/get-by-id/{id}` | `200 Ok` com `Dto` ou `404` |
| Criar | `POST` | `api/<modulo>/create` | `201 Created` com `Dto` |
| Atualizar | `PUT` | `api/<modulo>/update` | `200 Ok` com `Dto` |
| Excluir (soft) | `DELETE` | `api/<modulo>/delete/{id}` | `200 Ok` com mensagem |
| Reativar | `PATCH` | `api/<modulo>/activate/{id}` | `200 Ok` com mensagem |

### Controller Completo

```csharp
[ApiController]
[Route("api/minha-modulo")]
public class MinhaModuloController(IMediator mediator) : ControllerBase
{
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await mediator.Send(new GetMinhaEntidadesQuery(page, pageSize, search));
        return result.IsSuccess ? Ok(result.Value) : result.ToErrorResponse();
    }

    [HttpGet("get-by-id/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await mediator.Send(new GetMinhaEntidadeByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : result.ToErrorResponse();
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateMinhaEntidadeCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Created(string.Empty, result.Value) : result.ToErrorResponse();
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdateMinhaEntidadeCommand command)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : result.ToErrorResponse();
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await mediator.Send(new DeleteMinhaEntidadeCommand(id));
        return result.IsSuccess ? Ok(result.Value) : result.ToErrorResponse();
    }

    [HttpPatch("activate/{id:guid}")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await mediator.Send(new ActivateMinhaEntidadeCommand(id));
        return result.IsSuccess ? Ok(result.Value) : result.ToErrorResponse();
    }
}
```

---

## Testes

**Projeto**: `<NomeModulo>.Tests`
**Dependências**: `xUnit`, `FluentAssertions`, `NSubstitute`, `<NomeModulo>.Application`, `<NomeModulo>.Domain`

### Estrutura de pastas

```
<NomeModulo>.Tests/
├── Domain/
│   └── Entities/
│       └── MinhaEntidadeTests.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateMinhaEntidadeCommandHandlerTests.cs
│   │   ├── UpdateMinhaEntidadeCommandHandlerTests.cs
│   │   └── DeleteMinhaEntidadeCommandHandlerTests.cs
│   └── Queries/
│       ├── GetMinhaEntidadesQueryHandlerTests.cs
│       └── GetMinhaEntidadeByIdQueryHandlerTests.cs
└── _usings.cs
```

### Convenções de teste

- **Nome do teste**: `Metodo_Cenario_Resultado` (em inglês)
- **Padrão**: Arrange / Act / Assert
- **Mocks**: Moq
- **Assertions**: xUnit Assert (puro)

### Exemplo: Teste de Entidade (Domain)

```csharp
public class MinhaEntidadeTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateEntity()
    {
        // Arrange & Act
        var entidade = new MinhaEntidade("Nome Válido");

        // Assert
        entidade.Nome.Should().Be("Nome Válido");
        entidade.Active.Should().BeTrue();
        entidade.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowDomainException(string? nome)
    {
        // Arrange & Act
        var act = () => new MinhaEntidade(nome!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage(MinhaEntidadeMessages.Errors.NomeRequired);
    }

    [Fact]
    public void Deactivate_ShouldSetActiveFalse()
    {
        // Arrange
        var entidade = new MinhaEntidade("Teste");

        // Act
        entidade.Deactivate();

        // Assert
        entidade.Active.Should().BeFalse();
    }
}
```

### Exemplo: Teste de Handler (Application)

```csharp
public class CreateMinhaEntidadeCommandHandlerTests
{
    private readonly IMinhaEntidadeRepository _repository;
    private readonly CreateMinhaEntidadeCommandHandler _handler;

    public CreateMinhaEntidadeCommandHandlerTests()
    {
        _repository = Substitute.For<IMinhaEntidadeRepository>();
        _handler = new CreateMinhaEntidadeCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateMinhaEntidadeCommand("Nome Válido");
        _repository.ExistsByNomeAsync(command.Nome, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nome.Should().Be("Nome Válido");

        await _repository.Received(1).AddAsync(
            Arg.Any<MinhaEntidade>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnConflict()
    {
        // Arrange
        var command = new CreateMinhaEntidadeCommand("Nome Duplicado");
        _repository.ExistsByNomeAsync(command.Nome, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);

        await _repository.DidNotReceive().AddAsync(
            Arg.Any<MinhaEntidade>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnValidationError()
    {
        // Arrange
        var command = new CreateMinhaEntidadeCommand("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
    }
}
```

### _usings.cs

```csharp
global using Xunit;
global using FluentAssertions;
global using NSubstitute;
```

---

## Convenções Gerais

### Obrigatórias (definidas no CLAUDE.md)

- Todo `if`, `for`, `while`, `foreach` deve ter chaves `{}`, mesmo com uma única linha de corpo.

### Nomenclatura

| Elemento | Convenção | Exemplo |
|---|---|---|
| Entidade | PascalCase, singular | `Partner`, `Invoice` |
| Repositório (interface) | `I<Entidade>Repository` | `IPartnerRepository` |
| Repositório (classe) | `<Entidade>Repository` | `PartnerRepository` |
| Command | `<Verbo><Entidade>Command` | `CreatePartnerCommand` |
| Query | `Get<Entidade><Critério>Query` | `GetPartnerByIdQuery` |
| Query (listagem) | `Get<Entidade_Plural>Query` | `GetPartnersQuery` |
| Handler | `<Command/Query>Handler` | `CreatePartnerCommandHandler` |
| DTO | `<Entidade>Dto` | `PartnerDto` |
| DbContext | `<Módulo>DbContext` | `PartnersDbContext` |
| Messages | `<Entidade>Messages` | `PartnerMessages` |
| Configuration | `<Entidade>Configuration` | `PartnerConfiguration` |
| Controller | `<Módulo>Controller` | `PartnersController` |
| Teste | `<Classe>Tests` | `CreatePartnerCommandHandlerTests` |

### Idioma

- **Código** (nomes de classes, métodos, propriedades): inglês
- **Mensagens ao usuário** (constantes em `Messages`): português
- **Comentários e documentação** (arquivos `.md`): português

### Banco de dados

- Nomes de tabelas e colunas: `snake_case` (configurado via Fluent API)
- Cada módulo tem seu próprio `DbContext` e suas próprias migrations
- Nunca compartilhar DbContext entre módulos
- Soft delete: nunca remover registros fisicamente, usar `Active = false`
- Enums armazenados como `string` (via `HasConversion<string>()`)

### Tratamento de erros

- Fluxo de negócio: usar `Result<T>` — nunca `throw` para casos esperados
- Exceções de domínio (`DomainException`): apenas para violações de invariantes que representam bugs
- O controller é o único lugar que converte `Result` em resposta HTTP

### Soft Delete — Regras

- **Nunca** remover registros do banco. O "delete" é sempre `Active = false`.
- Repositórios filtram `Active == true` por padrão em todas as queries.
- Para reativação, existe um método explícito `GetByIdIncludingInactiveAsync`.
- O controller expõe `DELETE` para desativar e `PATCH activate` para reativar.

---

## Checklist para novo módulo

Ao criar um novo módulo, verificar:

- [ ] Pasta `Modules/<NomeModulo>/` criada
- [ ] Projetos `.Domain`, `.Application`, `.Infrastructure`, `.Tests` criados e referenciados na solution
- [ ] `Shared.Kernel` referenciado em Domain e Application
- [ ] Entidade(s) criada(s) herdando `BaseEntity`
- [ ] Construtor protegido vazio para EF Core em cada entidade
- [ ] Interface(s) de repositório definida(s) em Domain (sempre iniciar com `I`)
- [ ] Mensagens centralizadas em `<Entidade>Messages.cs`
- [ ] Enums criados com Extension Methods para `ToLabel()` (se aplicável)
- [ ] Commands implementados: Create, Update, Delete (soft), Activate
- [ ] Queries implementadas: GetAll (paginada), GetById
- [ ] Handlers com validação, verificação de duplicidade e retorno `Result<T>`
- [ ] DTOs definidos como records
- [ ] `DependencyInjection.cs` criado em Application e Infrastructure
- [ ] DbContext criado com `ApplyConfigurationsFromAssembly`
- [ ] Configuração Fluent API criada para cada entidade (snake_case, índices)
- [ ] `DomainEvents` ignorado na configuração Fluent API
- [ ] Repositório implementado com filtro `Active == true` por padrão
- [ ] Repositório com método `GetByIdIncludingInactiveAsync` para reativação
- [ ] Migration gerada
- [ ] Controller criado em WebApp com todos os endpoints (GET, POST, PUT, DELETE, PATCH)
- [ ] Rotas do controller usando `{id:guid}` para type safety
- [ ] Módulo registrado no `Program.cs`
- [ ] Testes de entidade (Domain) implementados
- [ ] Testes de handlers (Application) implementados com NSubstitute
- [ ] Arquivo `MODULO_<nome>.md` criado com documentação do módulo

---

## Módulos existentes

| Módulo | Documentação | Status |
|---|---|---|
| Partners | [MODULO_Partners.md](./MODULO_Partners.md) | Em produção |

---

### Mensagens relacionadas a transições inválidas

Centralizar no arquivo de mensagens do módulo:

```csharp
public static class SaleMessages
{
    public static class Errors
    {
        public const string InvalidStatusTransition =
            "Transição de status inválida para esta operação.";
    }
}
```

---

### Quando NÃO usar Enum

| Cenário | Use em vez disso |
|---|---|
| Valores que mudam com frequência (categorias cadastradas pelo usuário) | Entidade separada com tabela no banco |
| Mais de ~15 valores | Considerar tabela de lookup no banco |
| Valor precisa de dados complexos associados (cor, ícone, regras) | Value Object ou entidade auxiliar |

---

## Atualização no Checklist para novo módulo

Adicionar ao checklist existente:

- [ ] Enums criados em `<NomeModulo>.Domain/Enums/` (ou `Shared.Kernel/Enums/` se compartilhado)
- [ ] Enums com valores numéricos explícitos (iniciando em 1)
- [ ] Extension Method `ToLabel()` criado para cada enum com labels em português
- [ ] Configuração Fluent API com `.HasConversion<string>()` para cada propriedade enum
- [ ] DTO com campos `Status` (valor) e `StatusLabel` (label amigável)
- [ ] Métodos de transição de status na entidade (quando aplicável)

---

## Atualização na tabela de Nomenclatura

Adicionar à tabela existente de convenções:

| Elemento | Convenção | Exemplo |
|---|---|---|
| Enum | PascalCase, singular, sem prefixo | `SaleStatus`, `PaymentMethod` |
| Valores do Enum | PascalCase, descritivo, inglês | `Pending`, `Sold`, `NearExpiry` |
| Extension class | `<NomeEnum>Extensions` | `SaleStatusExtensions` |
| Método de label | `ToLabel()` | `status.ToLabel()` → `"Próximo do Vencimento"` |