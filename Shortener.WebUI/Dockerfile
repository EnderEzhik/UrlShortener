FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["./Shortener.WebUI.csproj", "./Shortener.WebUI.csproj"]
RUN dotnet restore "./Shortener.WebUI.csproj"
COPY . .
RUN dotnet build "Shortener.WebUI.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
RUN dotnet publish "./Shortener.WebUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
EXPOSE 5211
EXPOSE 7176

RUN mkdir -p /https

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Shortener.WebUI.dll"]
