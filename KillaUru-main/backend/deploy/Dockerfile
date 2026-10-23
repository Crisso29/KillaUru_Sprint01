# Imagen de la API (T027). Se construye desde la carpeta backend/:
#   docker buildx build --platform linux/arm64 -f deploy/Dockerfile -t ghcr.io/<cuenta>/killauru-api:<sha> .
# El SDK corre en la plataforma de quien construye y compila para la de destino (más rápido que emular arm64).
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacion
ARG TARGETARCH
WORKDIR /fuente
COPY . .
RUN dotnet publish src/KillaUru.Api/KillaUru.Api.csproj -c Release -a "$TARGETARCH" -o /publicado --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
# curl solo para el healthcheck de Docker contra /health/live
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=compilacion /publicado .
USER app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
HEALTHCHECK --interval=30s --timeout=3s --retries=3 CMD curl -fsS http://localhost:8080/health/live || exit 1
ENTRYPOINT ["dotnet", "KillaUru.Api.dll"]
