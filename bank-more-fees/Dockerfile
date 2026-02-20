# BankMore Fees - .NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/BankMore.Fees.Api/BankMore.Fees.Api.csproj", "BankMore.Fees.Api/"]
COPY ["src/BankMore.Fees.Application/BankMore.Fees.Application.csproj", "BankMore.Fees.Application/"]
COPY ["src/BankMore.Fees.Infrastructure/BankMore.Fees.Infrastructure.csproj", "BankMore.Fees.Infrastructure/"]
RUN dotnet restore "BankMore.Fees.Api/BankMore.Fees.Api.csproj"
COPY src/ .
WORKDIR "/src/BankMore.Fees.Api"
RUN dotnet build "BankMore.Fees.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Fees.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BankMore.Fees.Api.dll"]