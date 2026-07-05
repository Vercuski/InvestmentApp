#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InvestmentApp.Presentation.Web/InvestmentApp.Presentation.Web.csproj", "InvestmentApp.Presentation.Web/"]
COPY ["InvestmentApp.Application/InvestmentApp.Application.csproj", "InvestmentApp.Application/"]
COPY ["InvestmentApp.Domain/InvestmentApp.Domain.csproj", "InvestmentApp.Domain/"]
COPY ["InvestmentApp.Infrastructure/InvestmentApp.Infrastructure.csproj", "InvestmentApp.Infrastructure/"]
COPY ["InvestmentApp.Persistence/InvestmentApp.Persistence.csproj", "InvestmentApp.Persistence/"]
RUN dotnet restore "./InvestmentApp.Presentation.Web/InvestmentApp.Presentation.Web.csproj"
COPY . .
WORKDIR "/src/InvestmentApp.Presentation.Web"
RUN dotnet build "./InvestmentApp.Presentation.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InvestmentApp.Presentation.Web.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InvestmentApp.Presentation.Web.dll"]
