FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CareNotes.Server.slnx ./
COPY src/CareNotes.Api/CareNotes.Api.csproj src/CareNotes.Api/
COPY src/CareNotes.Application/CareNotes.Application.csproj src/CareNotes.Application/
COPY src/CareNotes.Domain/CareNotes.Domain.csproj src/CareNotes.Domain/
COPY src/CareNotes.Infrastructure/CareNotes.Infrastructure.csproj src/CareNotes.Infrastructure/
RUN dotnet restore src/CareNotes.Api/CareNotes.Api.csproj

COPY src/ src/
RUN dotnet publish src/CareNotes.Api/CareNotes.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "CareNotes.Api.dll"]
