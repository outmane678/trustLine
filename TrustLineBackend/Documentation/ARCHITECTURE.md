# TrustLineBackend — Architecture

## Structure du projet

```
TrustLineBackend/
│
├── Program.cs                        # Point d'entrée, enregistrement de tous les services
├── appsettings.json                  # Configuration (DB, JWT, Email, APIs externes, CORS)
├── web.config                        # Configuration IIS
│
├── Configurations/                   # Classes de configuration et extensions DI
│   ├── AppConfig.cs                  # Binding des sections appsettings
│   ├── JwtSettings.cs                # POCO pour les clés JWT
│   ├── JwtConfiguration.cs           # Enregistrement de l'auth JWT
│   ├── ChatSettings.cs               # Config SignalR/Chat
│   ├── CorsConfiguration.cs          # Politique CORS (origines frontend)
│   ├── DatabaseConfiguration.cs      # Enregistrement du DbContext EF Core
│   ├── SwaggerConfiguration.cs       # Config Swagger/OpenAPI
│   └── AddFileUploadParamOperationFilter.cs  # Filtre Swagger pour upload fichier
│
├── Controllers/                      # Points d'entrée HTTP (REST API)
│   ├── AnonymousComplaintsController.cs
│   ├── AttachmentsController.cs
│   ├── CategoriesController.cs
│   ├── DefendantsController.cs
│   ├── EmailController.cs
│   ├── ExternalUserController.cs
│   ├── FrequenciesController.cs
│   ├── HomeController.cs
│   ├── SolutionsController.cs
│   ├── TypeModelsController.cs
│   └── Users.cs
│
├── Data/                             # Couche accès données
│   ├── AnonymousComplaintsV002Context.cs   # DbContext EF Core
│   ├── Migrations/                   # Migrations EF Core
│   └── Script/script.sql             # Script SQL de référence
│
├── DTOs/                             # Objets de transfert (ni entités ni modèles DB)
│   ├── Requests/                     # Corps des requêtes HTTP entrantes
│   │   ├── CreateAnonymousComplaintRequest.cs
│   │   ├── MergeComplaintsRequest.cs
│   │   ├── PaginationRequest.cs
│   │   ├── SendEmailRequest.cs
│   │   ├── SendResponseRequest.cs
│   │   ├── UpdateCategoryRequest.cs
│   │   └── UserLoginRequest.cs
│   ├── Responses/                    # Corps des réponses HTTP sortantes
│   │   ├── AnonymousComplaintResponse.cs
│   │   ├── AttachmentResponse.cs
│   │   ├── CategoryResponse.cs
│   │   ├── ComplaintsByUserResponse.cs
│   │   ├── ComplaintWithUserDataResponse.cs
│   │   ├── DefendantResponse.cs
│   │   ├── FrequencyResponse.cs
│   │   ├── FullProfileResponseDto.cs
│   │   ├── PaginatedResponse.cs
│   │   ├── ShortProfileResponseDto.cs
│   │   ├── SolutionResponse.cs
│   │   ├── TypeModelResponse.cs
│   │   ├── UserResponse.cs
│   │   ├── UserTokenResponse.cs
│   │   └── ErrorViewModel.cs
│   ├── AccessGate/
│   │   └── PermissionCheckResponse.cs      # Réponse de vérification de permission
│   └── External/
│       └── ExternalUserDtos.cs             # DTOs utilisateurs externes
│
├── Models/
│   └── Entities/                     # Entités EF Core (tables DB)
│       ├── AnonymousComplaint.cs
│       ├── Attachment.cs
│       ├── Category.cs
│       ├── Frequency.cs
│       ├── Solution.cs
│       ├── Type.cs
│       └── User.cs
│
├── Repositories/                     # Couche accès données (pattern Repository)
│   ├── Interfaces/                   # Contrats
│   │   ├── IAnonymousComplaintRepository.cs
│   │   ├── IAttachmentRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   ├── IFrequencyRepository.cs
│   │   ├── ISolutionRepository.cs
│   │   ├── ITypeRepository.cs
│   │   └── IUserRepository.cs
│   └── Implementations/              # Implémentations EF Core
│       ├── AnonymousComplaintRepository.cs
│       ├── AttachmentRepository.cs
│       ├── CategoryRepository.cs
│       ├── FrequencyRepository.cs
│       ├── SolutionRepository.cs
│       ├── TypeRepository.cs
│       └── UserRepository.cs
│
├── Services/                         # Logique métier
│   ├── Interfaces/                   # Contrats
│   │   ├── IAnonymousComplaintService.cs
│   │   ├── IAttachmentService.cs
│   │   ├── ICategoryService.cs
│   │   ├── IEmailService.cs
│   │   ├── IFileService.cs
│   │   ├── IFrequencyService.cs
│   │   ├── ISolutionService.cs
│   │   ├── ITypeService.cs
│   │   ├── IAccessGateService.cs     # Vérification des permissions (API externe)
│   │   ├── IExternalUserService.cs   # Récupération utilisateurs (API externe)
│   │   └── IHrLinkService.cs         # Récupération profils RH (API externe)
│   ├── Implementations/              # Implémentations
│   │   ├── AnonymousComplaintService.cs
│   │   ├── AttachmentService.cs
│   │   ├── CategoryService.cs
│   │   ├── EmailService.cs
│   │   ├── FileService.cs
│   │   ├── FrequencyService.cs
│   │   ├── SolutionService.cs
│   │   ├── TypeService.cs
│   │   ├── AccessGateService.cs      # Appel HTTP vers AccessGate
│   │   ├── ExternalUserService.cs    # Appel HTTP vers AccessGate (users)
│   │   └── HrLinkService.cs          # Appel HTTP vers HrLink
│   ├── EmailService/
│   │   ├── IEmailService.cs
│   │   └── EmailService.cs
│   └── EnsureService/
│       └── EnsureUserService.cs      # Synchronisation/création utilisateur local
│
├── Mappers/                          # Conversion Entity ↔ DTO (sans AutoMapper)
│   ├── AnonymousComplaintMapper.cs
│   ├── AttachmentMapper.cs
│   ├── CategoryMapper.cs
│   ├── FrequencyMapper.cs
│   ├── SolutionMapper.cs
│   └── TypeModelMapper.cs
│
├── Middlewares/                      # Pipeline HTTP personnalisé
│   ├── ErrorHandlingMiddleware.cs    # Gestion globale des exceptions
│   ├── RequestLoggingMiddleware.cs   # Log de chaque requête entrante
│   └── MiddlewareExtensions.cs       # Extensions IApplicationBuilder
│
├── Helpers/                          # Utilitaires transversaux
│   ├── JwtHelpers.cs                 # Génération/lecture des tokens JWT
│   ├── AccessGatePermissionFilter.cs # Filtre d'action : vérifie une permission AccessGate
│   ├── ExternalServicePermissionFilter.cs
│   ├── CustomDateTimeConverter.cs
│   └── NoTrimStringModelBinder.cs
│
├── Hubs/
│   └── SolutionsHub.cs               # Hub SignalR (notifications temps réel)
│
├── Extensions/                       # Extensions de IServiceCollection
│   ├── RepositoryServiceExtensions.cs   # Enregistrement DI des repositories
│   ├── ServiceCollectionExtensions.cs   # Enregistrement DI des services
│   └── SessionExtensions.cs
│
├── Properties/
│   ├── launchSettings.json
│   └── PublishProfiles/              # Profils de publication IIS/Folder
│
└── wwwroot/
    └── uploads/ReclamationFiles/     # Fichiers joints uploadés (organisés par année/mois)
```

---

## Flux de données (Architecture en couches)

```
HTTP Request
    │
    ▼
[Middleware]          ErrorHandlingMiddleware, RequestLoggingMiddleware
    │
    ▼
[Controller]          Valide la requête, appelle le Service
    │
    ▼
[Service]             Logique métier, appelle le Repository ou les APIs externes
    │
    ├──▶ [Repository] ──▶ [DbContext EF Core] ──▶ SQL Server
    │
    └──▶ [Service Externe]  AccessGate / HrLink (via HttpClient)
    │
    ▼
[Mapper]              Entity → DTO de réponse
    │
    ▼
HTTP Response
```

---

## Appels aux services externes

Les services externes (AccessGate, HrLink) sont appelés via **`IHttpClientFactory`** avec un token JWT de service configuré dans `appsettings.json`.

### Configuration (`appsettings.json`)

```json
"ExternalApis": {
  "AccessGate": {
    "BaseUrl": "http://10.200.0.222:10100",
    "Token": "<JWT service token>",
    "AppCode": "TrustLine",
    "Endpoints": {
      "GetUsers": "/api/Profiles/Users",
      "GetUserById": "/api/authentication/GetUserById/{id}",
      "CheckPermission": "/api/external/permissions/check"
    }
  },
  "HrLink": {
    "BaseUrl": "http://10.200.0.222:10800",
    "Token": "<JWT service token>",
    "Endpoints": {
      "GetProfilesMinimal": "/api/external/Profiles/minimal",
      "GetProfileByUserId": "/api/external/profiles/ByUserId/{UserId}"
    }
  }
}
```

---

### AccessGate — Vérification de permission

**Fichier :** `Services/Implementations/AccessGateService.cs`  
**Interface :** `Services/Interfaces/IAccessGateService.cs`

**Fonctionnement :**
1. Le constructeur lit `BaseUrl`, `Token`, `AppCode` et `Endpoints:CheckPermission` depuis `IConfiguration`.
2. `CheckPermissionAsync(int userId, string permission)` construit l'URL :
   ```
   GET {BaseUrl}/api/external/permissions/check?userId={id}&permission={perm}&appCode=TrustLine
   ```
3. Le token JWT service est envoyé dans le header `Authorization: Bearer <token>`.
4. La réponse est désérialisée en `PermissionCheckResponse` (`DTOs/AccessGate/`).

**Utilisation dans un Controller (via filtre) :**
```csharp
[ServiceFilter(typeof(AccessGatePermissionFilter))]
// ou injection directe :
public async Task<IActionResult> MyAction([FromServices] IAccessGateService accessGate)
{
    bool allowed = await accessGate.CheckPermissionAsync(userId, "ag-ext-read-users");
    if (!allowed) return Forbid();
    // ...
}
```

---

### HrLink — Récupération des profils RH

**Fichier :** `Services/Implementations/HrLinkService.cs`  
**Interface :** `Services/Interfaces/IHrLinkService.cs`

**Méthodes disponibles :**

| Méthode | Endpoint | Description |
|---|---|---|
| `GetProfilesMinimalAsync()` | `GET /api/external/Profiles/minimal` | Liste de tous les profils (version courte) |
| `GetProfileByUserIdAsync(userId)` | `GET /api/external/profiles/ByUserId/{UserId}` | Profil complet d'un utilisateur |

**Fonctionnement :**
1. Même pattern qu'AccessGate : `IHttpClientFactory` + token Bearer.
2. Retourne `List<ShortProfileResponseDto>` ou `FullProfileResponseDto`.

**Utilisation :**
```csharp
public class MyService
{
    private readonly IHrLinkService _hrLink;

    public MyService(IHrLinkService hrLink) => _hrLink = hrLink;

    public async Task DoSomething()
    {
        var profiles = await _hrLink.GetProfilesMinimalAsync();
        var profile  = await _hrLink.GetProfileByUserIdAsync(userId);
    }
}
```

---

### ExternalUserService — Récupération des utilisateurs AccessGate

**Fichier :** `Services/Implementations/ExternalUserService.cs`  
**Interface :** `Services/Interfaces/IExternalUserService.cs`

Appelle `AccessGate` pour lister ou récupérer des utilisateurs  
(endpoints `GetUsers` / `GetUserById`).

---

## Enregistrement DI

Tous les services et repositories sont injectés dans `Program.cs` via les extensions :

```csharp
// Extensions/RepositoryServiceExtensions.cs
builder.Services.AddRepositories();

// Extensions/ServiceCollectionExtensions.cs
builder.Services.AddApplicationServices();

// IHttpClientFactory (requis par les services externes)
builder.Services.AddHttpClient();
```

---

## Authentification JWT (interne)

- Configuration : `appsettings.json` → section `JsonWebTokenKeys`
- Setup : `Configurations/JwtConfiguration.cs`
- Génération de token : `Helpers/JwtHelpers.cs`
- Les endpoints internes utilisent `[Authorize]` standard ASP.NET Core.
