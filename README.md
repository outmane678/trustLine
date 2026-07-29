# TrustLine Backend

ASP.NET Core 8 — API de gestion des plaintes anonymes.

## Structure

| Projet | Description |
|---|---|
| `TrustLineBackend` | API principale |
| `TrustLine.UnitTests` | Tests unitaires (xUnit + Moq) |
| `TrustLine.IntegrationTests` | Tests d'intégration (xUnit + WebApplicationFactory) |

## Lancer les tests

```bash
dotnet test Project.sln
```

## CI/CD

Jenkins déclenche automatiquement le pipeline à chaque push via GitHub webhook.
