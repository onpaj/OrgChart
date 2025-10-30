# Deployment Summary - MSAL Runtime Configuration

Implementace runtime konfigurace pro MSAL authentication byla dokončena:

## Změny

1. ✅ **Backend config endpoint** (`/api/config`) - vrací MSAL konfiguraci z environment variables
2. ✅ **Frontend dynamické načítání** - volá config endpoint při startu aplikace  
3. ✅ **MSAL inicializace** - používá dynamickou konfiguraci
4. ✅ **Fallback mechanismus** - v případě nedostupnosti config endpointu (jen development)
5. ✅ **Security** - žádné credentials v public Docker image

## Pro Azure Web App nastav tyto Application Settings:

### Backend Authentication
```
AzureAd__TenantId=31fd4df1-b9c0-4abd-a4b0-0e1aceaabe9a
AzureAd__ClientId=9a82ce31-1912-4edb-9617-32cf77c903a6  
AzureAd__ClientSecret=your-backend-secret
AzureAd__Audience=api://9a82ce31-1912-4edb-9617-32cf77c903a6
```

### Frontend Configuration  
```
Frontend__ClientId=92b5ce3d-d69c-43a1-aa39-28ffe58e265e
```

### Optional
```
UseMockAuth=false
OrgChart__DataSourceType=AzureStorage
```

## Jak to funguje:

1. **Production**: Frontend volá `/api/config` pro získání MSAL konfigurace
2. **Development**: Fallback na `REACT_APP_AZURE_*` environment variables
3. **Automatic challenge**: Provede se hned při načtení stránky pro neautentizované uživatele
4. **Security**: Config endpoint je označen `[AllowAnonymous]` pro dostupnost bez autentizace

## Hlavní výhody:

- **Bezpečnost**: Docker image neobsahuje žádné credentials
- **Flexibilita**: Jeden Docker image funguje ve všech prostředích
- **Automatizace**: Uživatel je automaticky přesměrován na přihlášení
- **Runtime konfigurace**: Změny konfigurace bez rebuildu image

Docker image zůstává čistý bez credentials - konfigurace se načítá za běhu z Azure Web App Application Settings.