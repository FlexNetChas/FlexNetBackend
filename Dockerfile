# Multi-stage build DockerImage for production
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md

# ARG - Variable that we can pass information to the build process.
# We choosen ARG's to make it easy to update the .NET version in one place
ARG DOTNET_VERSION=8.0
ARG BUILD_CONFIGURATION=Release
ARG EF_CORE_VERSION=9.0.0

# Stage 1: Build
## Create build stage from the official .NET SDK image and give the stage a name
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build

ARG BUILD_CONFIGURATION
ENV BUILD_CONFIGURATION=${BUILD_CONFIGURATION}

## Copy csproj-filer into build and restore dependencies
WORKDIR /src
COPY ["src/FlexNet.Api/FlexNet.Api.csproj", "FlexNet.Api/"]
COPY ["src/FlexNet.Application/FlexNet.Application.csproj", "FlexNet.Application/"]
COPY ["src/FlexNet.Domain/FlexNet.Domain.csproj", "FlexNet.Domain/"]
COPY ["src/FlexNet.Infrastructure/FlexNet.Infrastructure.csproj", "FlexNet.Infrastructure/"]

WORKDIR /src/FlexNet.Api
RUN dotnet restore "FlexNet.Api.csproj"

## Copy all source code
WORKDIR /src
COPY . .

## Build and publish application. Flag publish RID to linux-x64 to produce a self-contained deployment
WORKDIR /src/src/FlexNet.Api
RUN dotnet build "FlexNet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "FlexNet.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish -r linux-x64 --self-contained false

# Stage 2: Migrations
## This stage installs EF Core tools and prepares for running migrations
FROM build AS migrate
ARG EF_CORE_VERSION 
ARG BUILD_CONFIGURATION

## Install EF Core tools globally. EF Core Tools need to match EF Core packages version used in the project
RUN dotnet tool install --global dotnet-ef --version ${EF_CORE_VERSION}

ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src/src/FlexNet.Api

## Build the project to ensure assemblies are available for migrations
RUN dotnet build "FlexNet.Api.csproj" -c ${BUILD_CONFIGURATION}
ENTRYPOINT ["dotnet", "ef", "database", "update"]

# Stage 3: Runtime
## To reduce the .NET SDK image large file size do we want to run the published files from the .NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:$DOTNET_VERSION AS runtime

ENV DOTNET_RUNNING_IN_CONTAINER=true

## Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

## Create a non-root user to run the application for better security
RUN adduser --disabled-password --gecos '' --no-create-home appuser

WORKDIR /app
COPY --from=build /app/publish . 

## Create entrypoint bash-script to check for connetionstring and start the application
RUN echo '#!/bin/bash\n\
set -e\n\
echo "Starting FlexNet Backend API..."\n\
if [ -z "$ConnectionStrings__DefaultConnection" ]; then\n\
    echo "ConnectionStrings is not set in .env.backend"\n\
fi\n\
echo "Launching application"\n\
exec dotnet FlexNet.Api.dll' > /app/entrypoint.sh && \
    chmod +x /app/entrypoint.sh

## Change ownership of the /app folder to the non-root user for better security
RUN chown -R appuser:appuser /app

## Just for documentation purposes, the container listens on port 8080
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
USER appuser


# Commands Summary
#─────────────────────────────────────────────────────
#  Build image - <tag:version> <file:path>
#- docker build -t flexnet-backend:latest -f Dockerfile.prod . 
#─────────────────────────────────────────────────────
#  Run container - <port:host> <name:container>
#- docker run -p 5000:8080 --name flexnet-backend flexnet-backend:latest
#─────────────────────────────────────────────────────
#  Stop container - <name:container>
#- docker stop flexnet-backend
#─────────────────────────────────────────────────────
#  Start container - <name:container>
#- docker start flexnet-backend
#─────────────────────────────────────────────────────
#  Remove container - <name:container>
#- docker rm flexnet-backend
#─────────────────────────────────────────────────────
#  List running containers
#- docker ps
#─────────────────────────────────────────────────────
#  List all containers
#- docker ps -a
#─────────────────────────────────────────────────────
#  Remove image - <tag:version>
#- docker rmi flexnet-backend:latest
#─────────────────────────────────────────────────────
#  Scan image for vulnerabilities using Docker Scout - <tag:version>
#- docker scout quickview flexnet-backend:latest
#─────────────────────────────────────────────────────
#  Remove not running Docker containers, networks, images, volumes
#- docker system prune -a --volumes -f
#─────────────────────────────────────────────────────
#  Remove builder cache and cache layers
#- docker builder prune -a -f --all
