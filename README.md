# Api Rest Com Autenticacao JWT Lista Tarefas ToDo

API REST (ToDo) usando .NET 8, EF Core (Code First), SQL Server via Docker e autenticação JWT

## 1) Criar solução e projeto Web API

```bash
mkdir TodoApp && cd TodoApp
dotnet new sln -n TodoApp

# Criar projeto Web API
dotnet new webapi -n TodoApi
dotnet sln add TodoApi/TodoApi.csproj

# (opcional) projeto de testes
dotnet new xunit -n TodoApi.Tests
dotnet sln add TodoApi.Tests/TodoApi.Tests.csproj
```

## 2) Pacotes NuGet necessários

```bash
cd TodoApi
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.8
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.8
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 8.0.8
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.8
dotnet add package Swashbuckle.AspNetCore --version 8.0.8
# para testes: Moq, FluentAssertions etc (se quiser)
```

## 3) Estrutura de pastas recomendada

Dentro do projeto TodoApi, crie:

```bash
/Controllers
/Models/Domain
/Models/DTOs
/Data
/Repositories
/Services
/Helpers (AutoMapper, JWT settings)
```

## 4) Models (entidades)

```Models/Domain/User.cs```

```cs
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
}
```

```Models/Domain/TodoItem.cs```

```cs
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // relacionamento
    public int UserId { get; set; }
    public User? User { get; set; }
}
```

### ✅ O que são essas classes?

Essas classes são entidades de domínio, também chamadas de models no contexto do Entity Framework Core.

O EF Core usa essas classes para:

- Criar tabelas no banco
- Definir colunas
- Mapear relacionamentos
- Ler/escrever dados entre o banco e o seu código

Ou seja: elas representam as tabelas e suas relações.

### 🔎 Entidade User

#### Detalhando:

- ```public int Id { get; set; }```
  - É a primary key da tabela ```Users```.
  - Por convenção, o EF Core reconhece automaticamente ```Id``` como chave primária.
  - Ele será auto‐incrementado no SQL Server.
- ```public string Username { get; set; } = null!;```
  - Armazena o nome de usuário.
  - ```= null!``` significa:
    - O compilador quer evitar string nulo (por causa do Nullable Reference Types),
    - mas você está avisando: "Eu garanto que esse valor não será nulo".
- ```public string PasswordHash { get; set; } = null!;```
  - Aqui vai a senha criptografada (nunca a senha real).
  - Normalmente gerada com bcrypt/argon2, etc.
- public string Email { get; set; } = null!;
  - Armazena o email.
  - Também obrigatório.

### 🔎 Entidade TodoItem

- ```public int Id { get; set; }```
  - Chave primária da tabela TodoItems.
- ```public string Title { get; set; } = null!;```
  - Título da tarefa.
  - Obrigatório.
- ```public string? Description { get; set; }```
  - string? significa permitir nulo.
  - Pode ou não ter descrição.
- ```public bool IsCompleted { get; set; } = false;```
  - Indica se a tarefa foi concluída.
  - Por padrão começa como false.
- ```public DateTime CreatedAt { get; set; } = DateTime.UtcNow;```
  - Armazena a data/hora de criação.
  - Define automaticamente o valor no momento em que a entidade é criada.

### 🔗 RELACIONAMENTO ENTRE User e TodoItem

As duas últimas propriedades definem um relacionamento 1:N (um usuário → muitas tarefas):

- public int UserId { get; set; }
  - Chave estrangeira.
  - Indica a qual usuário a tarefa pertence.
  - No banco será criada uma coluna ```UserId```.
- ```public User? User { get; set; }```
  - Essa é a propriedade de navegação.
  - Permite acessar o usuário dono da tarefa:

```cs
todo.User.Email
```

- O ? indica que ela pode ser nula até o EF carregar os dados (lazy/eager loading).

### 🧠 Como o EF Core enxerga esse relacionamento?

Ele entende automaticamente:

- User tem muitos TodoItem
- TodoItem tem um User
- A FK é UserId

Equivalente a isto no SQL Server:

```sql
FOREIGN KEY (UserId) REFERENCES Users(Id)
```

### 📌 Em resumo

| Entidade            | Representa         | Observações                                  |
| ------------------- | ------------------ | -------------------------------------------- |
| **User**            | Tabela de usuários | Tem Id, Username, Senha criptografada, Email |
| **TodoItem**        | Tabela de tarefas  | Pertence a um usuário via UserId             |
| **User → TodoItem** | Relacionamento 1:N | Um usuário pode ter muitas tarefas           |

Claro, Lucas — vou te explicar **exatamente** como o Entity Framework Core usa essas entidades para **gerar as tabelas no banco via migrations**, passo a passo e sem enrolação.

---

### ✅ **1) O EF lê suas entidades e monta um “modelo interno”**

Quando você executa:

```bash
dotnet ef migrations add Initial
```

O EF Core:

1. Lê todas as classes que estão no seu `DbContext` (no seu caso: `User` e `TodoItem`).
2. Analisa as propriedades e tipos.
3. Identifica chaves primárias, relacionamentos, nulabilidade, tipos C#, defaults, etc.
4. Constrói um **modelo de banco de dados virtual**, chamado *model snapshot*.

Esse “modelo interno” vira a **base** para gerar as tabelas SQL.

---

### ✅ **2) O EF Core converte esse modelo em comandos SQL**

Depois de criar o modelo, o EF gera um **arquivo de migração** na pasta:

```bash
Migrations/
```

Esse arquivo contém métodos principais:

#### 👉 `Up()`

Cria tabelas, colunas, FKs…

#### 👉 `Down()`

Desfaz (deleta tabelas) — útil para rollback.

---

### ⚙️ **3) A migração gerada para User ficaria mais ou menos assim:**

```cs
migrationBuilder.CreateTable(
    name: "Users",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Username = table.Column<string>(nullable: false),
        PasswordHash = table.Column<string>(nullable: false),
        Email = table.Column<string>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Users", x => x.Id);
    });
```

#### O EF concluiu

- `Id` → PRIMARY KEY
- `string` → nvarchar(max) NOT NULL
- `string?` seria → nvarchar(max) NULL
- Defaults não são criados a menos que você configure (ou via fluent API)

---

### ⚙️ **4) A migração gerada para TodoItem ficaria assim:**

```cs
migrationBuilder.CreateTable(
    name: "TodoItems",
    columns: table => new
    {
        Id = table.Column<int>(nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Title = table.Column<string>(nullable: false),
        Description = table.Column<string>(nullable: true),
        IsCompleted = table.Column<bool>(nullable: false),
        CreatedAt = table.Column<DateTime>(nullable: false),
        UserId = table.Column<int>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_TodoItems", x => x.Id);
        table.ForeignKey(
            name: "FK_TodoItems_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });
```

Observe os pontos importantes:

#### 1. `UserId` virou uma **foreign key**

Porque você tem:

```cs
public int UserId { get; set; }
public User? User { get; set; }
```

#### 2. EF criou automaticamente

- Índice para `UserId`
- FK com delete cascade

#### 3. `Description` virou nullable (porque tem `string?`)

#### 4. `CreatedAt` virou coluna com tipo `datetime2`

---

### 🧩 **5) Quando você aplica a migração**

Você roda:

```bash
dotnet ef database update
```

O EF:

- abre a conexão com o SQL Server
- executa todos os comandos SQL dentro de `Up()`
- cria as tabelas no banco

---

### 🗄️ **6) Resultado final no SQL Server**

#### Tabela: **Users**

| Coluna       | Tipo          | Null | Observação   |
| ------------ | ------------- | ---- | ------------ |
| Id           | int           | No   | PK, identity |
| Username     | nvarchar(max) | No   | obrigatório  |
| PasswordHash | nvarchar(max) | No   | obrigatório  |
| Email        | nvarchar(max) | No   | obrigatório  |

---

### Tabela: **TodoItems**

| Coluna      | Tipo          | Null | Observação              |
| ----------- | ------------- | ---- | ----------------------- |
| Id          | int           | No   | PK, identity            |
| Title       | nvarchar(max) | No   | obrigatório             |
| Description | nvarchar(max) | Yes  | opcional                |
| IsCompleted | bit           | No   | false default no código |
| CreatedAt   | datetime2     | No   | default pelo C#         |
| UserId      | int           | No   | FK → Users.Id           |

---

#### FK

```cs
TodoItems.UserId → Users.Id
```

---

### ⭐ Em resumo

1. **Entities** → o EF Core interpreta as classes C#
2. **DbContext** → ele descobre quais entidades existem
3. **Migration** → EF converte tudo em SQL
4. **database update** → SQL Server cria as tabelas reais

## Explicando os seguintes topicos

1. **Como o EF Core descobre nomes de tabelas/colunas por convenção**
2. **Como sobrescrever tudo usando Fluent API**
3. **Como gerar o SQL real das migrations sem aplicar no banco**

---

### ✅ **1) COMO O EF CORE DESCOBRE TUDO POR CONVENÇÃO**

O EF Core tem um conjunto de regras automáticas chamadas **convenções** (conventions).
Ele usa essas regras para decidir **como será o nome da tabela, coluna, chave primária, chave estrangeira, relacionamento, tamanho de campo...** sem você ter que configurar.

Vou detalhar as convenções mais importantes.

---

#### 📌 **1.1 – Nome da Tabela**

Por padrão:

- Nome da tabela = nome da classe
- No plural se você usar `DbSet<>`

Exemplo:

```csharp
public DbSet<User> Users { get; set; }
```

Tabela se chamará:

```bash
Users
```

Se você chamar:

```csharp
public DbSet<TodoItem> TodoItems { get; set; }
```

Tabela:

```bash
TodoItems
```

Se você remover o DbSet, o EF ainda cria a tabela usando o nome da classe.

---

#### 📌 **1.2 – Nome das Colunas**

Coluna = nome da propriedade

Exemplo:

```csharp
public string PasswordHash { get; set; }
```

Coluna:

```bash
PasswordHash
```

---

#### 📌 **1.3 – Chave Primária**

Se uma propriedade se chamar:

- `Id`
- `UserId`
- `TodoItemId`
- `{NomeDaClasse}Id`

O EF assume que é **primary key**.

No caso:

```csharp
public int Id { get; set; }
```

Virou:

```bash
PRIMARY KEY (Id)
```

---

#### 📌 **1.4 – Chave Estrangeira (FK)**

Se uma classe tiver:

```csharp
public int UserId { get; set; }
public User User { get; set; }
```

O EF automaticamente entende:

- `UserId` é FK → tabela Users
- Relacionamento: 1 User → N TodoItems

---

#### 📌 **1.5 – Nullability (se pode ser nulo ou não)**

- Tipos **não anuláveis** (`string`, `int`, `bool`) → NOT NULL
- Tipos anuláveis (`string?`, `int?`, `bool?`) → NULL

Exemplo:

```csharp
public string? Description { get; set; } 
```

Tabela:

```bash
Description nvarchar(max) NULL
```

---

#### 📌 **1.6 – Tipos C# → Tipos SQL**

O EF faz o mapeamento sozinho:

| C#       | SQL Server    |
| -------- | ------------- |
| int      | int           |
| string   | nvarchar(max) |
| bool     | bit           |
| DateTime | datetime2     |
| decimal  | decimal(18,2) |

---

### ✅ **2) COMO SOBRESCREVER TUDO (FLUENT API)**

Isso é feito no método `OnModelCreating()` dentro do seu `ApplicationDbContext`.

Aqui você controla **tabela, coluna, tamanho, relacionamento, chave primária, índices e muito mais.**

---

#### ✔ 2.1 — Configurar nome da tabela

```csharp
modelBuilder.Entity<User>()
    .ToTable("Usuarios");
```

---

#### ✔ 2.2 — Configurar nome da coluna

```csharp
modelBuilder.Entity<User>()
    .Property(u => u.Username)
    .HasColumnName("Login");
```

---

#### ✔ 2.3 — Configurar tamanho e obrigatoriedade

```csharp
modelBuilder.Entity<User>()
    .Property(u => u.Username)
    .HasMaxLength(100)
    .IsRequired();
```

---

#### ✔ 2.4 — Configurar a chave primária manualmente

```csharp
modelBuilder.Entity<User>()
    .HasKey(u => u.Id);
```

---

#### ✔ 2.5 — Configurar relacionamento explicitamente

```csharp
modelBuilder.Entity<TodoItem>()
    .HasOne(t => t.User)
    .WithMany()        // ou .WithMany(u => u.TodoItems) se você tivesse uma lista
    .HasForeignKey(t => t.UserId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

#### ✔ 2.6 — Índice

```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();
```

---

#### ✔ 2.7 — Default value no banco

```csharp
modelBuilder.Entity<TodoItem>()
    .Property(t => t.CreatedAt)
    .HasDefaultValueSql("GETUTCDATE()");
```

(O padrão no seu código é no C#, mas isso aqui faz no próprio SQL Server.)

---

### 📌 **Exemplo completo de Fluent API para suas entidades**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.ToTable("Users");

        entity.Property(u => u.Username)
              .HasMaxLength(100)
              .IsRequired();

        entity.Property(u => u.Email)
              .IsRequired();

        entity.HasIndex(u => u.Email)
              .IsUnique();
    });

    modelBuilder.Entity<TodoItem>(entity =>
    {
        entity.ToTable("TodoItems");

        entity.Property(t => t.Title)
              .HasMaxLength(200)
              .IsRequired();

        entity.Property(t => t.Description)
              .HasMaxLength(1000);

        entity.HasOne(t => t.User)
              .WithMany()
              .HasForeignKey(t => t.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });
}
```

---

### ✅ **3) COMO GERAR O SQL REAL SEM APLICAR NO BANCO**

Muito útil quando você quer ver o SQL antes de rodar.

Comando:

```bash
dotnet ef migrations script
```

Isso:

- Lê todas as migrations
- Gera o SQL final
- Mas **não aplica** no banco

Resultado:

Um arquivo `.sql` gigante com todos os comandos de criação das tabelas.

---

### 📌 Gerar script só da migração mais recente

```bash
dotnet ef migrations script 0 Initial
```

Ou:

```bash
dotnet ef migrations script Initial AddTodoItem
```

---

#### 📌 Gerar script e salvar em arquivo

```bash
dotnet ef migrations script -o estrutura.sql
```

---

### 🎯 **RESUMO RÁPIDO**

#### 1) O EF descobre tudo sozinho (convenção)

- Nome da tabela = nome da classe
- Colunas = propriedades
- FK = `UserId` + `User`
- Tipo C# define tipo SQL
- `string?` define coluna `NULL`

#### 2) Você pode sobrescrever tudo com Fluent API

- Tabela, coluna, tamanhos, índices
- Relacionamentos completos
- Defaults no SQL Server
- Delete behavior

#### 3) Dá para gerar o SQL real

- `dotnet ef migrations script`

## 5) DbContext

O objeto desse trecho é a classe ApplicationDbContext, que é uma subclasse de DbContext do Entity Framework Core.

Em resumo, ele representa a conexão com o banco de dados + os conjuntos de tabelas (DbSet).

### ✔ O que exatamente é esse objeto?

É um objeto de contexto de banco de dados do EF Core.

### Ele faz três coisas

1. Abre a conexão com o banco de dados (via DbContextOptions<\ApplicationDbContext>).
2. Mapeia as entidades do seu domínio para tabelas do banco:
    - Users → tabela Users
    - TodoItems → tabela TodoItems
3. Permite executar operações de CRUD:
    - context.Users.Add(...)
    - context.Users.FirstOrDefault(...)
    - context.TodoItems.ToList()
    - context.SaveChanges()

### ✔ Então o objeto é

> Um DbContext configurado para gerenciar as tabelas Users e TodoItems dentro do banco de dados.

Ele é uma ponte entre seu código C# e o SQL Server.

```Data/ApplicationDbContext.cs```

```cs
using Microsoft.EntityFrameworkCore;
using TodoApi.Models.Domain;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
}
```
