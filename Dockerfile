# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MklinkUi.sln ./
COPY src ./src

RUN dotnet restore
RUN dotnet build src/MklinkUi.Fakes -c Release
RUN dotnet publish src/MklinkUi.WebUI -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MklinkUi.WebUI.dll"]
