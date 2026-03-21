FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["ARFurniture.AdminWeb/ARFurniture.AdminWeb.csproj", "ARFurniture.AdminWeb/"]
RUN dotnet restore "ARFurniture.AdminWeb/ARFurniture.AdminWeb.csproj"

COPY . .
WORKDIR "/src/ARFurniture.AdminWeb"
RUN dotnet publish "ARFurniture.AdminWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet ARFurniture.AdminWeb.dll"]
