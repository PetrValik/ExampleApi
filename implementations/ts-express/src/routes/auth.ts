import { Router } from 'express';
import jwt from 'jsonwebtoken';

import { config } from '../config';

/**
 * POST /auth/token — demo credential exchange.
 *
 * Hardcoded demo credentials `admin` / `admin` (documented stand-in for a real
 * identity provider). Valid credentials mint an HS256 JWT carrying a `name` claim,
 * issuer, audience and expiry; anything else returns a bare 401.
 */
export const authRouter: Router = Router();

const DEMO_USERNAME = 'admin';
const DEMO_PASSWORD = 'admin';

authRouter.post('/auth/token', (req, res) => {
  const body = (req.body ?? {}) as { username?: unknown; password?: unknown };
  const username = typeof body.username === 'string' ? body.username : '';
  const password = typeof body.password === 'string' ? body.password : '';

  if (username !== DEMO_USERNAME || password !== DEMO_PASSWORD) {
    res.status(401).end();
    return;
  }

  const expiresAt = new Date(Date.now() + config.jwt.expirationMinutes * 60_000);

  const token = jwt.sign({ name: username }, config.jwt.secret, {
    algorithm: 'HS256',
    issuer: config.jwt.issuer,
    audience: config.jwt.audience,
    subject: username,
    expiresIn: `${config.jwt.expirationMinutes}m`,
  });

  res.status(200).json({ token, expiresAt: expiresAt.toISOString() });
});
