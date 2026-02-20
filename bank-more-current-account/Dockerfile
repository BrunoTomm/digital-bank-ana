# BankMore Current Account - .NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os projetos
COPY ["src/BankMore.CurrentAccount.Api/BankMore.CurrentAccount.Api.csproj", "BankMore.CurrentAccount.Api/"]
COPY ["src/BankMore.CurrentAccount.Application/BankMore.CurrentAccount.Application.csproj", "BankMore.CurrentAccount.Application/"]
COPY ["src/BankMore.CurrentAccount.Domain/BankMore.CurrentAccount.Domain.csproj", "BankMore.CurrentAccount.Domain/"]
COPY ["src/BankMore.CurrentAccount.Infrastructure/BankMore.CurrentAccount.Infrastructure.csproj", "BankMore.CurrentAccount.Infrastructure/"]

# Restaura dependências
RUN dotnet restore "BankMore.CurrentAccount.Api/BankMore.CurrentAccount.Api.csproj"

# Copia o restante do código
COPY src/ .
WORKDIR "/src/BankMore.CurrentAccount.Api"

# Compila em Release
RUN dotnet build "BankMore.CurrentAccount.Api.csproj" -c Release -o /app/build

# Publica
FROM build AS publish
RUN dotnet publish "BankMore.CurrentAccount.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.CurrentAccount.Api.dll"]