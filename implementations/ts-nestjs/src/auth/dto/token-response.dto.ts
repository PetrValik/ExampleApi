/** Response for POST /auth/token (camelCase per the wire contract). */
export interface TokenResponse {
  token: string;
  expiresAt: string;
}
