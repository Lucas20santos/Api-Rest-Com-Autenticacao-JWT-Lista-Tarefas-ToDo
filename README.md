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
