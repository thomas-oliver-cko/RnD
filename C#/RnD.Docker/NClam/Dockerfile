# Build image
FROM mcr.microsoft.com/dotnet/sdk:3.1-alpine AS build
ARG app=Files.AvScan.Api
ARG Version=0.0.0
COPY . /app/
WORKDIR /app/
RUN dotnet publish "/app/src/${app}/${app}.csproj" -o output -c Release /p:Version=${Version} /p:GenerateDocumentationFile=false /p:Verbosity=diag

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:3.1-alpine as dotnet-clam

RUN apk update && \
    apk add --no-cache clamav bash curl && \
    mkdir /run/clamav && \
    chown clamav /run/clamav && \
    chmod 750 /run/clamav
    
RUN freshclam

FROM dotnet-clam
ARG app=Files.AvScan.Api
ARG owner=Marketplace
ARG Version=0.0.0

LABEL "cko.app.owner"=${owner} "cko.app"=${app} "cko.app.version"=${Version}
ENV appName=${app}

WORKDIR /app
COPY --from=build /app/output/ .

EXPOSE 5000
EXPOSE 5001

ENTRYPOINT [ "/bin/sh", "/app/entrypoint.sh" ]
