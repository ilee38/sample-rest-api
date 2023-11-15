# List of pending items to implement ("tech debt")

## Installers
- Add the installers for all services for dependency injection

## Domain to contract mapping
- implement Automapper for the reminder of the domain objects

## Fluent validation
- implement validation for the post requests (currently only implemented for tag endpoints)

## ApiKey Authentication
- Decide where tu use it and apply it to controllers. Currently, only the middleware class is implemented (under  `/Filters`)

## Redis
- Test the caching feature with a redis cache running in Docker (see YT video at the end)

## Docker
- Make the Dockerfile and docker-compose files work