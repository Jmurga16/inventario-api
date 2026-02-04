# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Inventario.Domain/Inventario.Domain.csproj", "Inventario.Domain/"]
COPY ["Inventario.Application/Inventario.Application.csproj", "Inventario.Application/"]
COPY ["Inventario.Infrastructure/Inventario.Infrastructure.csproj", "Inventario.Infrastructure/"]
COPY ["Inventario.Api/Inventario.Api.csproj", "Inventario.Api/"]

RUN dotnet restore "Inventario.Api/Inventario.Api.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Inventario.Api"
RUN dotnet build "Inventario.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Inventario.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Copy published files
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check (uses wget which is available in the base image)
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Inventario.Api.dll"]
