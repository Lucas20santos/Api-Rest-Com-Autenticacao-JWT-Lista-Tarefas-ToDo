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
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore
# para testes: Moq, FluentAssertions etc (se quiser)
```
