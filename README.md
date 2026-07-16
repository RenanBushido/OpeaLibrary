# OpeaLibrary
This is a project for Opea interview.
Itś a library systems made with:

-   .NET 10
-   EntityFrameworkCore
-   MongoClient
-   Docker
-   xUnit
-   IMapper
-   MediatR
-   Claude Code
-   OpenSpec


# Getting start

## Requirements

-   Docker installed
-   Docker Compose installed
-   .NET (SDK) 10 installed

## Fist Steps

Clone the project: 

https://github.com/RenanBushido/OpeaLibrary.git

Load the databases, using docker-compose.yml file, inside the folder OpeaLibrary/src/docker-compose.yml

### `docker compose up -d`

### `docker ps -a`

Be sure that databases are loaded.

Inside the folder OpeaLibrary/src/Presentation

### `dotnet run` 

Open the browser following: http://localhost:5000/swagger



