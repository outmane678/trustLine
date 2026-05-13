# TrustLine Backend — Integration & Changes Report

> **Project**: TrustLine (AnonymousComplaintsAPI)  
> **Version**: V2.0.0  
> **Date**: March 2026

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Feature 1 — Centralized Permission Check via AccessGate](#2-feature-1--centralized-permission-check-via-accessgate)
3. [Feature 2 — External Service API for User Management](#3-feature-2--external-service-api-for-user-management)

---

## 1. Executive Summary

Two major structural changes have been applied to the TrustLine backend:

1. **Centralized access control via AccessGate** — Every API endpoint is now guarded by a real-time permission check against the central AccessGate server. Users without the required permission receive a **403 Forbidden** response.

2. **External Service API for user management** — A new controller (`ExternalUserController`) exposes CRUD endpoints at `api/external/users` for consumption by other ecosystem applications (AccessGate, etc.) using a secure service token.

Both changes bring TrustLine in line with the standard architecture already adopted by every other application in the ecosystem (KPI360, ExpedPro, SmartScreen, HrLink, StockWise, etc.).

---

## 2. Feature 1 — Centralized Permission Check via AccessGate

### 2.1 Background

Prior to this change, TrustLine endpoints had no granular authorization — any authenticated user could call any endpoint regardless of their role.

### 2.2 How It Works

Every endpoint is now decorated with `[RequirePermission("tl-xxx")]`. At runtime, before the controller action executes, the filter calls AccessGate's External API to verify that the calling user holds the required permission.

```
Browser ──[user JWT]──▶ TrustLine Backend ──[service token]──▶ AccessGate
                               │
                       AccessGatePermissionFilter
                       calls /api/external/permissions/check
```

**Step-by-step flow:**

1. The JWT middleware validates the user's token (signature + lifetime).
2. `AccessGatePermissionFilter` extracts the user ID from the `"Id"` claim in the JWT.
3. `AccessGateService` sends an HTTP GET to AccessGate:  
   `GET /api/external/permissions/check?userId={id}&permission={perm}&appCode=TrustLine`
4. AccessGate resolves the permission across global roles, project roles, direct grants, and delegations.
5. If `hasPermission = true` → the action executes normally.  
   If `hasPermission = false` → **403 Forbidden** is returned.

### 2.3 Permission Reference (tl-\*)

These permissions are managed in AccessGate and checked on every endpoint call:

| Permission | Description | Guarded Endpoints |
|------------|-------------|-------------------|
| `tl-v-report` | View own complaints | GET complaints (by user) |
| `tl-v-reportmanagement` | View all complaints (management) | GET all, GET details, GET fused, GET solutions, GET attachments |
| `tl-u-report` | Update a complaint | PUT, ChangeState, upload attachments |
| `tl-d-report` | Delete/archive a complaint | DELETE, archive/restore complaints, solutions, attachments |
| `tl-m-report` | Merge complaints | POST merge |
| `tl-r-report` | Respond to a complaint | POST/PUT solutions, send email |
| `tl-v-category` | View categories | GET categories (paginated, by type, by ID) |
| `tl-c-category` | Create a category | POST category |
| `tl-u-category` | Update a category | PUT category |
| `tl-a-category` | Archive/restore a category | PATCH archive/restore |
| `tl-d-category` | Hard-delete a category | DELETE |
| `tl-v-type` | View types and frequencies | GET types, GET frequencies |
| `tl-c-type` | Create a type or frequency | POST type/frequency |
| `tl-u-type` | Update a type or frequency | PUT type/frequency |
| `tl-a-type` | Archive/restore a type or frequency | PATCH archive/restore |
| `tl-d-type` | Hard-delete a type or frequency | DELETE |

### 2.4 Controllers Modified (50 endpoints guarded)

| Controller | Endpoints | Permissions Used |
|------------|-----------|-----------------|
| `AnonymousComplaintsController` | 11 | tl-v-reportmanagement, tl-v-report, tl-u-report, tl-d-report, tl-m-report |
| `AttachmentsController` | 9 | tl-v-reportmanagement, tl-u-report, tl-d-report |
| `SolutionsController` | 6 | tl-v-reportmanagement, tl-r-report, tl-d-report |
| `CategoriesController` | 9 | tl-v-category, tl-c-category, tl-u-category, tl-a-category, tl-d-category |
| `TypeModelsController` | 8 | tl-v-type, tl-c-type, tl-u-type, tl-a-type, tl-d-type |
| `FrequenciesController` | 7 | tl-v-type, tl-c-type, tl-u-type, tl-a-type, tl-d-type |
| `EmailController` | 1 | tl-r-report |

### 2.5 Configuration

```json
"ExternalApis": {
    "AccessGate": {
        "BaseUrl": "http://10.200.0.222:10100",
        "Token": "<TrustLine service token with ag-ext-check-permissions>",
        "AppCode": "TrustLine",
        "Endpoints": {
            "CheckPermission": "/api/external/permissions/check"
        }
    }
}
```

### 2.6 Security Properties

| Property | Detail |
|----------|--------|
| **Default policy** | Deny — if AccessGate is unreachable or returns an error, access is denied |
| **Timeout** | 10 seconds per permission check call |
| **Service token** | Long-lived JWT carrying `ag-ext-check-permissions`, stored in `appsettings.json` |
| **Fail-safe** | Any exception during the check results in `return false` (access denied) |

### 2.7 Files Created / Modified

| File | Status | Role |
|------|--------|------|
| `Helpers/AccessGatePermissionFilter.cs` | New | `[RequirePermission]` attribute + async action filter |
| `Services/Interfaces/IAccessGateService.cs` | New | Interface for the AccessGate HTTP client |
| `Services/Implementations/AccessGateService.cs` | New | HTTP GET to AccessGate's permission check endpoint |
| `DTOs/AccessGate/PermissionCheckResponse.cs` | New | DTO mapping AccessGate's JSON response |
| `Extensions/ServiceCollectionExtensions.cs` | Modified | DI registration for the new service |
| `appsettings.json` | Modified | AccessGate base URL, token, appCode, endpoints |
| 7 controllers | Modified | Added `[Authorize]` + `[RequirePermission]` on every endpoint |

---

## 3. Feature 2 — External Service API for User Management

### 3.1 Background

Other applications in the ecosystem need to manage users in the TrustLine database programmatically — for example, provisioning a new employee's account when they are created in AccessGate, or archiving a user across all applications simultaneously.

### 3.2 Authentication — Service Token

The External API uses the same authentication model as AccessGate's own External API:

- The calling application authenticates with a **service token** — a long-lived JWT issued by AccessGate.
- The token is sent in the `Authorization: Bearer <token>` header on every request.
- The token carries `tl-ext-*` permissions in its `role` claims.
- TrustLine validates the JWT signature (same signing key as user tokens) and reads the permissions directly from the claims — **no additional HTTP call is needed**.

**Service token claims (decoded):**

| Claim | Example Value | Description |
|-------|---------------|-------------|
| `token_type` | `"service"` | Distinguishes it from a user token |
| `app_id` | `"3"` | AccessGate internal app ID of the caller |
| `app_name` | `"AccessGate"` | Human-readable app name |
| `user_id` | `"1109"` | Service account user ID |
| `NameIdentifier` | `"1109"` | Same as `user_id` — used by the filter to identify the caller |
| `role` | `["tl-ext-read-users", ...]` | Permissions the token grants |

### 3.3 Permission Model

Each endpoint is guarded by a named `tl-ext-*` permission. The service token must carry the corresponding permission in its `role` claims, otherwise the endpoint returns **403 Forbidden**.

| Permission | Guards | Description |
|------------|--------|-------------|
| `tl-ext-read-users` | `GET users`, `GET users/{id}` | Read user records |
| `tl-ext-create-users` | `POST users` | Create new users in TrustLine |
| `tl-ext-update-users` | `PUT users/{id}` | Update user data |
| `tl-ext-archive-users` | `PATCH users/{id}/archive` | Soft-delete (archive) a user |
| `tl-ext-restore-users` | `PATCH users/{id}/restore` | Re-enable an archived user |

### 3.4 How Permission Check Differs from Feature 1

| Aspect | Feature 1 (User-facing) | Feature 2 (External API) |
|--------|-------------------------|--------------------------|
| **Caller** | End-user via the frontend | Another application via a service token |
| **Token type** | User token (claim `"Id"` = user ID) | Service token (claim `NameIdentifier` = service account ID) |
| **How permission is checked** | HTTP call to AccessGate in real time | Direct read from JWT `role` claims (no network call) |
| **Permission prefix** | `tl-*` (e.g. `tl-v-report`) | `tl-ext-*` (e.g. `tl-ext-read-users`) |
| **Attribute** | `[RequirePermission("tl-v-report")]` | `[RequireExternalPermission("tl-ext-read-users")]` |

### 3.5 Endpoint Reference

#### 3.5.1 Get All Users

```
GET /api/external/users
GET /api/external/users?includeArchived=true
```

**Required permission**: `tl-ext-read-users`

**Response 200**

```json
[
  { "userId": 42, "archived": false },
  { "userId": 55, "archived": false }
]
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `includeArchived` | bool | No | Default `false`. Set to `true` to include archived users. |

---

#### 3.5.2 Get User by ID

```
GET /api/external/users/{userId}
```

**Required permission**: `tl-ext-read-users`

**Response 200**

```json
{ "userId": 42, "archived": false }
```

**Errors**: `404` if user not found.

---

#### 3.5.3 Create User

```
POST /api/external/users
Content-Type: application/json
```

**Required permission**: `tl-ext-create-users`

**Request**

```json
{ "userId": 42 }
```

| Field | Required | Description |
|-------|----------|-------------|
| `userId` | Yes | Must be a positive integer. Should match the AccessGate user ID. |

**Response 201 Created**

```json
{ "userId": 42, "archived": false }
```

**Errors**: `400` if userId ≤ 0. `409` if user already exists.

---

#### 3.5.4 Update User

```
PUT /api/external/users/{userId}
Content-Type: application/json
```

**Required permission**: `tl-ext-update-users`

**Request**

```json
{ "archived": true }
```

| Field | Required | Description |
|-------|----------|-------------|
| `archived` | No | Set to `true` or `false`. Omit (`null`) to leave unchanged. |

**Response 200** — updated user.

**Errors**: `404` if user not found.

---

#### 3.5.5 Archive User

Soft-delete a user. The record is retained but excluded from active listings. Reversed by [Restore User](#356-restore-user).

```
PATCH /api/external/users/{userId}/archive
```

**Required permission**: `tl-ext-archive-users`

**Response 204 No Content** on success.

**Errors**: `404` if user not found. `409` if already archived.

---

#### 3.5.6 Restore User

Re-enable an archived user.

```
PATCH /api/external/users/{userId}/restore
```

**Required permission**: `tl-ext-restore-users`

**Response 204 No Content** on success.

**Errors**: `404` if user not found. `409` if not currently archived.

---

### 3.6 Error Reference

| HTTP Status | When Returned |
|-------------|---------------|
| 200 OK | Success (GET, PUT) |
| 201 Created | User created successfully (POST) |
| 204 No Content | Archive/Restore successful (PATCH) |
| 400 Bad Request | Invalid parameter (e.g. userId ≤ 0) |
| 401 Unauthorized | No `Authorization` header, invalid token, or token expired |
| 403 Forbidden | Token is valid but does not carry the required `tl-ext-*` permission |
| 404 Not Found | User does not exist |
| 409 Conflict | User already exists (POST), already archived (archive), or not archived (restore) |
| 500 Internal Server Error | Unexpected server-side failure |

All error responses include a JSON body:

```json
{
  "error": "Human-readable message"
}
```

### 3.7 User Model Change

The `Archived` field (bool) has been added to the `User` entity:

```csharp
public partial class User
{
    public int UserId { get; set; }
    public bool Archived { get; set; }    // ← New
    // ... navigation properties ...
}
```

> **Action required**: Add the `Archived` column (`BIT DEFAULT 0`) to the `Users` table in the database, or run an EF Core migration.

### 3.8 Configuration for Calling Applications

The external application must store the service token in its configuration:

```json
{
    "ExternalApis": {
        "TrustLine": {
            "BaseUrl": "http://localhost:5062",
            "Token": "<service_token_with_tl-ext-*_permissions>"
        }
    }
}
```

And send the `Authorization: Bearer <token>` header on every request.

### 3.9 Files Created

| File | Role |
|------|------|
| `Controllers/ExternalUserController.cs` | Controller with 7 endpoints (CRUD + archive/restore) |
| `Helpers/ExternalServicePermissionFilter.cs` | `[RequireExternalPermission]` attribute + filter (reads JWT claims) |
| `Services/Interfaces/IExternalUserService.cs` | Business service interface |
| `Services/Implementations/ExternalUserService.cs` | Implementation (DTO mapping, business logic) |
| `Repositories/Interfaces/IUserRepository.cs` | Repository interface |
| `Repositories/Implementations/UserRepository.cs` | Database access layer (Entity Framework Core) |
| `DTOs/External/ExternalUserDtos.cs` | DTOs: `ExternalUserResponse`, `CreateExternalUserRequest`, `UpdateExternalUserRequest` |

---


*Generated for the TrustLine project — March 2026*
