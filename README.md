# Api Rest Com Autenticacao JWT Lista Tarefas ToDo

API REST (ToDo) usando .NET 8, EF Core (Code First), SQL Server via Docker e autenticação JWT

## Criar solução e projeto Web API

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
