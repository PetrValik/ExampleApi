import type { NextFunction, Request, Response } from 'express';
import jwt from 'jsonwebtoken';

import { config } from '../config';

/**
 * JWT bearer guard for the protected /api/articles/** routes.
 *
 * Requires `Authorization: Bearer <jwt>`; a missing or invalid token yields a bare
 * 401 (no body), matching the contract. The token is verified against the same
 * secret/issuer/audience used to mint it, with HS256 pinned as the only algorithm.
 */
export function requireAuth(req: Request, res: Response, next: NextFunction): void {
  const header = req.header('authorization') ?? req.header('Authorization');

  if (!header || !header.toLowerCase().startsWith('bearer ')) {
    res.status(401).end();
    return;
  }

  const token = header.slice(header.indexOf(' ') + 1).trim();
  if (token.length === 0) {
    res.status(401).end();
    return;
  }

  try {
    jwt.verify(token, config.jwt.secret, {
      algorithms: ['HS256'],
      issuer: config.jwt.issuer,
      audience: config.jwt.audience,
    });
    next();
  } catch {
    res.status(401).end();
  }
}
