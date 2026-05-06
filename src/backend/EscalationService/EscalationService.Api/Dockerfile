FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/backend/EscalationService/EscalationService.Api/EscalationService.Api.csproj", "src/backend/EscalationService/EscalationService.Api/"]
RUN dotnet restore "src/backend/EscalationService/EscalationService.Api/EscalationService.Api.csproj"
COPY . .
WORKDIR "/src/src/backend/EscalationService/EscalationService.Api"
RUN dotnet build "EscalationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EscalationService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EscalationService.Api.dll"]
