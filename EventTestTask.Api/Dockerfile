FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY EventTestTask.Api/EventTestTask.Api.csproj EventTestTask.Api/
COPY EventTestTask.Application/EventTestTask.Application.csproj EventTestTask.Application/
COPY EventTestTask.Core/EventTestTask.Core.csproj EventTestTask.Core/
COPY EventTestTask.Infrastructure/EventTestTask.Infrastructure.csproj EventTestTask.Infrastructure/

RUN dotnet restore EventTestTask.Api/EventTestTask.Api.csproj

COPY . .

FROM build AS publish
RUN dotnet publish EventTestTask.Api/EventTestTask.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "EventTestTask.Api.dll"]
