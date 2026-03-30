# Módulo Partners

Gerencia os parceiros de negócio da empresa — pessoas físicas (CPF) ou jurídicas (CNPJ). É o módulo base do ERP, pois outros módulos (pedidos, financeiro, etc.) referenciam parceiros como clientes, fornecedores ou transportadoras.

> Leia [ARCHITECTURE.md](./ARCHITECTURE.md) para convenções gerais antes de modificar este módulo.

---

## Localização no projeto

```
ErpIxact/Modules/Patners/          ← typo histórico na pasta, não renomear
├── Partners.Domain
├── Partners.Application
└── Partners.Infrastructure
```

> A pasta se chama `Patners` (typo), mas os namespaces usam `Patners.*` consistentemente. Não renomear a pasta para não quebrar as migrations e referências existentes.

---

## Modelo de domínio

### Entidade `Partners`

**Arquivo**: `Partners.Domain/Entities/Partners.cs`

| Propriedade | Tipo | Regras |
|---|---|---|
| `Id` | `Guid` (v7) | Gerado automaticamente em `BaseEntity` |
| `DocNumber` | `DocNumber` (Value Object) | Obrigatório; CPF ou CNPJ válido; único na tabela |
| `Name` | `string` | Obrigatório; não pode ser vazio ou espaço |
| `Active` | `bool` | `true` por padrão (definido em `BaseEntity`) |
| `CreatedAt` | `DateTime` (UTC) | Definido em `BaseEntity` na criação |
| `UpdatedAt` | `DateTime?` (UTC) | Nulo até a primeira atualização |

**Invariantes de domínio** (validadas no construtor e em `Update`):
- `DocNumber` nulo → lança `DomainException(PartnersMessages.Errors.DocNumberRequired)`
- `Name` nulo/vazio → lança `DomainException(PartnersMessages.Errors.NameRequired)`
- DocNumber inválido (CPF/CNPJ com dígitos errados) → `DocNumber` já lança `ArgumentException` internamente

**Comportamentos**:
- `Update(docNumber, name, active)` — atualiza os três campos e chama `SetUpdatedAt()`
- O construtor protegido vazio existe exclusivamente para o EF Core

### Value Object `DocNumber`

**Arquivo**: `Shared.Kernel/ValueObjects/DocNumber.cs`

- Armazena apenas os dígitos (sem pontuação) em `Value`
- Aceita entrada com ou sem formatação — `StringFunctions.ExtractDigits` normaliza
- Valida CPF (11 dígitos) e CNPJ (14 dígitos) com algoritmo de módulo 11
- Rejeita sequências homogêneas (ex: `111.111.111-11`)
- `Formatted` retorna CPF como `000.000.000-00` e CNPJ como `00.000.000/0000-00`
- Igualdade baseada em `Value` (apenas dígitos)

---

## Banco de dados

**Tabela**: `partners`
**DbContext**: `PartnersDbContext`
**Arquivo de configuração**: `Partners.Infrastructure/Data/Configurations/PartnersConfiguration.cs`

| Coluna | Tipo PostgreSQL | Restrições |
|---|---|---|
| `Id` | `uuid` | PK |
| `doc_number` | `varchar(14)` | NOT NULL, UNIQUE (`ix_partners_doc_number`) |
| `name` | `varchar(200)` | NOT NULL |
| `created_at` | `timestamp without time zone` | NOT NULL |
| `updated_at` | `timestamp without time zone` | NULL |
| `active` | `boolean` | NOT NULL |

**Detalhe de mapeamento**: `DocNumber` é um Value Object ignorado pelo EF (`builder.Ignore(p => p.DocNumber)`). O campo privado `_docNumber` (só dígitos) é mapeado diretamente via `PropertyAccessMode.Field`.

**Migrations aplicadas**:
1. `20260319210950_Initial` — cria tabela `partners` com índice único em `doc_number`
2. `20260319214757_260319-1-UpdatePartners` — renomeia índice para `ix_partners_doc_number`
3. `20260330205148_260330-1-TimestampWithoutTimezone` — altera `created_at` e `updated_at` para `timestamp without time zone`

**Gerar nova migration**:
```bash
dotnet ef migrations add <NomeDaMigration> \
  --project Modules/Patners/Partners.Infrastructure \
  --startup-project WebApp
```

---

## Repositório

**Interface**: `Partners.Domain/Repositories/IPartnersRepository.cs`
**Implementação**: `Partners.Infrastructure/Repositories/PartnersRepository.cs`

| Método | Comportamento |
|---|---|
| `GetByIdAsync(id)` | `FindAsync` — tracking ativado (usado antes de Update) |
| `GetByDocNumberAsync(docNumber)` | Extrai dígitos antes de comparar; `AsNoTracking` |
| `GetByNameAsync(name)` | `LIKE %name%` case-sensitive; `AsNoTracking` |
| `GetAllAsync()` | Sem filtro; `AsNoTracking` |
| `AddAsync(partner)` | `AddAsync` + `SaveChangesAsync` |
| `UpdateAsync(partner)` | `Update` + `SaveChangesAsync` |

> `GetByDocNumberAsync` usa `EF.Property<string>(p, "_docNumber")` para acessar o campo privado na query SQL.

---

## Application — Commands e Queries

### Commands

#### `CreatePartnerCommand`
**Arquivo**: `Commands/CreatePartner/`

```
entrada:  string DocNumber, string Name
saída:    Result<PartnerDto>
```

Fluxo:
1. Busca parceiro existente com mesmo `DocNumber`
2. Se já existe → `Result.Conflict(AlreadyExists)`
3. Cria `DocNumber` value object (lança se inválido)
4. Cria entidade `Partners`
5. Persiste via `AddAsync`
6. Retorna `Result.Success(PartnerDto)` com `DocNumber.Formatted`

#### `UpdatePartnerCommand`
**Arquivo**: `Commands/UpdatePartner/`

```
entrada:  Guid Id, string DocNum, string Name, bool Active
saída:    Result<PartnerDto>
```

Fluxo:
1. Busca por `Id` — se não existe → `Result.NotFound(NotFound)`
2. Busca por `DocNum` — se existe e pertence a outro parceiro → `Result.Conflict(AlreadyExists)`
3. Chama `existing.Update(docNum, name, active)`
4. Persiste via `UpdateAsync`
5. Retorna `Result.Success(PartnerDto)`

### Queries

| Query | Entrada | Saída | Comportamento quando vazio |
|---|---|---|---|
| `GetPartnersQuery` | — | `Result<List<PartnerDto>>` | Retorna lista vazia com sucesso |
| `GetPartnerByIdQuery` | `Guid Id` | `Result<PartnerDto>` | `Result.NotFound` |
| `GetPartnerByDocNumberQuery` | `string DocNumber` | `Result<PartnerDto>` | `Result.NotFound` |
| `GetPartnerByNameQuery` | `string Name` | `Result<List<PartnerDto>>` | `Result.NotFound` |

> `GetPartnersQuery` (lista completa) retorna sucesso mesmo com lista vazia. `GetPartnerByNameQuery` retorna `NotFound` se não encontrar nenhum resultado.

### DTO

```csharp
public record PartnerDto(Guid Id, string DocNumber, string Name, bool Active);
```

`DocNumber` no DTO é sempre formatado (`DocNumber.Formatted`), nunca só os dígitos.

---

## API REST

**Controller**: `WebApp/Controllers/PartnersController.cs`
**Rota base**: `api/Partners`

| Método | Rota | Body | Retorno sucesso |
|---|---|---|---|
| `GET` | `/partner/get-all` | — | `200 Ok` + lista |
| `GET` | `/partner/get-by-id/{id:guid}` | — | `200 Ok` + parceiro |
| `GET` | `/partner/get-by-doc/{docNumber}` | — | `200 Ok` + parceiro |
| `GET` | `/partner/get-by-name/{name}` | — | `200 Ok` + lista |
| `POST` | `/partner/create` | `{ docNumber, name }` | `201 Created` + parceiro |
| `PUT` | `/partner/update/{id:guid}` | `{ id, docNum, name, active }` | `200 Ok` + parceiro |

**Validação extra no PUT**: se `id` da rota ≠ `command.Id` do body → `400 Bad Request` imediato, sem chamar o MediatR.

**Mapeamento de erros**:

| `ErrorType` | HTTP |
|---|---|
| `NotFound` | `404 Not Found` |
| `Conflict` | `409 Conflict` |
| demais | `400 Bad Request` |

Corpo do erro: `{ "error": "mensagem" }`

---

## Mensagens

**Arquivo**: `Partners.Domain/Messages/PartnersMessages.cs`

| Constante | Valor |
|---|---|
| `Errors.DocNumberRequired` | `"Número documento é obrigatório."` |
| `Errors.DocNumberInvalid` | `"Número documento inválido."` |
| `Errors.NameRequired` | `"Nome do parceiro é obrigatório."` |
| `Errors.NotFound` | `"Parceiro não encontrado."` |
| `Errors.AlreadyExists` | `"Parceiro já cadastrado com este documento."` |
| `Alerts.DocNumberChanged` | `"O documento do parceiro foi alterado."` |
| `Success.Created` | `"Parceiro criado com sucesso."` |
| `Success.Updated` | `"Parceiro atualizado com sucesso."` |
| `Success.Deleted` | `"Parceiro removido com sucesso."` |

---

## Registro de dependências

**Program.cs**:
```csharp
builder.Services.AddPartnersApplication();
builder.Services.AddPartnersInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection")!);
```

**`AddPartnersApplication`** registra: handlers MediatR via `RegisterServicesFromAssembly`

**`AddPartnersInfrastructure`** registra: `PartnersDbContext` (Npgsql) + `IPartnersRepository → PartnersRepository` (Scoped)

---

## Pontos de atenção para futuras modificações

- **Exclusão lógica**: não existe endpoint de DELETE. Para desativar um parceiro, usar o `PUT` com `active: false`.
- **Busca por nome**: usa `LIKE` nativo do PostgreSQL — sensível a maiúsculas/minúsculas. Se precisar de busca case-insensitive, usar `EF.Functions.ILike`.
- **DocNumber no EF**: o campo privado `_docNumber` é mapeado via shadow property. Qualquer mudança na configuração do EF exige nova migration.
- **Namespace vs pasta**: o namespace é `Patners.*` (com o typo). Manter consistente.
