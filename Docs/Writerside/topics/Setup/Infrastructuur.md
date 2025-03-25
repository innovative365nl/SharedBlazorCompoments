# Infrastructuur

## Overzicht
Automate365 draait op een Azure-gebaseerde infrastructuur met verschillende componenten die nauw samenwerken. Alle infrastructuur wordt gedefinieerd als Infrastructure as Code (IaC) via Bicep en wordt automatisch uitgerold via GitHub Actions.

## Core Componenten

### Management Portal
- Azure Web App hosting de Blazor applicatie
- Fungeert als primair gebruikersinterface
- Communiceert met de Management API

### Management API
- Azure API Management (APIM) instance
- Centrale hub voor alle service communicatie
- Handelt authenticatie en autorisatie af
- Beheert API versioning en documentatie

### Azure Functions
- Meerdere function apps voor verschillende services
- Self-registration bij de Management API
- Specifieke functionaliteit per service
- Event-driven architectuur

## Ondersteunende Services

### Azure Key Vault
- Centrale opslag voor secrets en certificaten
- Bevat database connection strings
- API keys en andere gevoelige configuratie
- Managed Identities voor secure access

### Azure SQL Server
- Centrale database server
- Meerdere databases voor verschillende services:
    - Tenant database
    - Service-specifieke databases

### Azure Service Bus
- asynchroon messaging systeem tussen azure functions

### Log Analytics Workspace
- Centrale logging oplossing
- Application Insights integratie

## Deployment

### Infrastructure as Code
- Bicep templates in repository: `/infrastructure`
- Modulaire opbouw per component
- Environment-specifieke parameters
- Volledig geautomatiseerde uitrol

### GitHub Actions
- Automatische deployment pipeline - [Github-Actions](https://github.com/innovative365nl/Automate/blob/develop/.github/workflows/Deployment-Infrastructure-Azure.yml)
- Environment-based deployments (dev/acc/prod)
- Infrastructure full mode. Alles wort ook verwijdered
