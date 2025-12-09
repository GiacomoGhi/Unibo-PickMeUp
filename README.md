# PickMeUp

Applicazione di car pooling.

## Prerequisiti

Prima di avviare l'applicazione, assicurati di avere:

1. **.NET 10 SDK** installato

   - Verifica con: `dotnet --version`
   - Download: https://dotnet.microsoft.com/download

2. **Docker** installato e il daemon attivo

   - Verifica con: `docker --version`
   - Download: https://docs.docker.com/get-docker/

3. **Credenziali Google API** configurate
   - Apri `src/PickMeUp.Web/appsettings.json`
   - Sostituisci i placeholder in `Authentication.Google`:
     - `ClientId`: la tua Client ID di Google OAuth
     - `MapsApiKey`: la tua API Key per Google Maps

## Avvio dell'applicazione

### Windows

```cmd
startup.bat
```

### Linux/macOS/Git Bash

```bash
chmod +x startup.sh
./startup.sh
```

Lo script avvierà automaticamente:

- PostgreSQL (porta 5432)
- Mailpit (SMTP: 1025, Web UI: http://localhost:8025)
- L'applicazione web PickMeUp

## Servizi

Una volta avviata, l'applicazione sarà disponibile su:

- **Web App**: http://localhost:5178
- **Mailpit UI**: http://localhost:8025 (per visualizzare le email di sviluppo)
- **Database**: localhost:5432 (PostgreSQL)
