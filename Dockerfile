# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Restore first (better layer caching)
COPY src/OrderApi/OrderApi.csproj src/OrderApi/
RUN dotnet restore src/OrderApi/OrderApi.csproj

# Copy the rest and publish
COPY src/OrderApi/ src/OrderApi/
RUN dotnet publish src/OrderApi/OrderApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# .NET 8 images ship with a non-root "app" user - use it (security best practice)
USER app

ENTRYPOINT ["dotnet", "OrderApi.dll"]
