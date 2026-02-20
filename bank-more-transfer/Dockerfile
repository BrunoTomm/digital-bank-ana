# BankMore Transfer - .NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Transfer.Api/BankMore.Transfer.Api.csproj", "BankMore.Transfer.Api/"]
COPY ["src/BankMore.Transfer.Application/BankMore.Transfer.Application.csproj", "BankMore.Transfer.Application/"]
COPY ["src/BankMore.Transfer.Domain/BankMore.Transfer.Domain.csproj", "BankMore.Transfer.Domain/"]
COPY ["src/BankMore.Transfer.Infrastructure/BankMore.Transfer.Infrastructure.csproj", "BankMore.Transfer.Infrastructure/"]
RUN dotnet restore "BankMore.Transfer.Api/BankMore.Transfer.Api.csproj"
COPY src/ .
WORKDIR "/src/BankMore.Transfer.Api"
RUN dotnet build "BankMore.Transfer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Transfer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Transfer.Api.dll"]