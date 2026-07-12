/**
 * Body for POST /auth/token.
 *
 * Intentionally undecorated: invalid/empty credentials must yield **401**
 * (handled by {@link AuthService}), not a 400 validation error.
 */
export class TokenRequestDto {
  username?: string;
  password?: string;
}
