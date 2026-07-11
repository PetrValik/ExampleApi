# GetToken — `POST /auth/token`

Issues a signed JWT bearer token for the demo user. This is the only anonymous write
endpoint; every article write endpoint requires the token this slice produces.

> **Demo only.** Credentials are hardcoded (`admin` / `admin`) as a stand-in for a real
> identity provider. Swap `GetTokenHandler.Handle` for a real user store before production
> (which would typically make the handler asynchronous).

## Request

```http
POST /auth/token
Content-Type: application/json

{
  "username": "admin",
  "password": "admin"
}
```

| Field      | Type   | Required | Notes                    |
|------------|--------|----------|--------------------------|
| `username` | string | yes      | Demo value: `admin`      |
| `password` | string | yes      | Demo value: `admin`      |

## Responses

| Status | When                        | Body                                            |
|--------|-----------------------------|-------------------------------------------------|
| `200`  | Credentials valid           | `{ "token": "<jwt>", "expiresAt": "<utc>" }`    |
| `401`  | Credentials invalid/missing | `application/problem+json`                       |

The token is an HMAC-SHA256 JWT carrying a `name` claim, signed with the configured
`Jwt:SecretKey`, valid for `Jwt:ExpirationMinutes` (default 60). Send it as
`Authorization: Bearer <token>` on protected requests.

## Slice anatomy

| File                   | Role                                                          |
|------------------------|--------------------------------------------------------------|
| `GetTokenEndpoint.cs`  | Thin HTTP surface: bind → handler → map (`200`/`401`)        |
| `GetTokenHandler.cs`   | Credential check + JWT issuance (the only logic)             |
| `IGetTokenHandler.cs`  | Handler contract (DI + testability)                          |
| `TokenRequest.cs`      | Request DTO                                                  |
| `TokenResponse.cs`     | Response DTO (`token`, `expiresAt`)                          |

The handler returns `null` for invalid credentials so the endpoint maps it to `401`
without any HTTP concern leaking into the handler.
