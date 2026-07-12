/**
 * Central application configuration, sourced from environment variables with
 * sensible development defaults so the app compiles and runs without a full
 * environment. Production values are injected via docker-compose.
 */

function readInt(value: string | undefined, fallback: number): number {
  if (value === undefined || value.trim() === '') {
    return fallback;
  }
  const parsed = Number.parseInt(value, 10);
  return Number.isNaN(parsed) ? fallback : parsed;
}

export interface JwtConfig {
  secret: string;
  issuer: string;
  audience: string;
  expirationMinutes: number;
}

export interface AppConfig {
  port: number;
  jwt: JwtConfig;
}

// The demo signing key must be at least 32 chars for HMAC-SHA256; the default
// below is a development-only placeholder and is overridden in docker-compose.
const DEFAULT_SECRET = 'CHANGE-THIS-IN-PRODUCTION-use-a-long-random-secret-key';

export const config: AppConfig = {
  port: readInt(process.env.PORT, 8080),
  jwt: {
    secret: process.env.JWT_SECRET ?? DEFAULT_SECRET,
    issuer: process.env.JWT_ISSUER ?? 'ExampleApi',
    audience: process.env.JWT_AUDIENCE ?? 'ExampleApiClient',
    expirationMinutes: readInt(process.env.JWT_EXPIRATION_MINUTES, 60),
  },
};

if (config.jwt.secret.length < 32) {
  throw new Error(
    `JWT secret must be at least 32 characters for HMAC-SHA256. Current length: ${config.jwt.secret.length}.`,
  );
}
