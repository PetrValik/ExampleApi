import express, { type Express } from 'express';

import { errorHandler, jsonSyntaxErrorHandler } from './middleware/errorHandler';
import { articlesRouter } from './routes/articles';
import { authRouter } from './routes/auth';
import { healthRouter } from './routes/health';

/**
 * Builds the Express application: JSON body parsing, the anonymous routes
 * (health, auth) first, then the JWT-guarded article routes, then the
 * problem+json error handlers last so every failure is normalised.
 */
export function createApp(): Express {
  const app = express();

  app.disable('x-powered-by');
  app.use(express.json());

  // Anonymous routes.
  app.use(healthRouter);
  app.use(authRouter);

  // Protected article routes (the router applies the bearer guard itself).
  app.use(articlesRouter);

  // Error normalisation — registered last so it catches everything above.
  app.use(jsonSyntaxErrorHandler);
  app.use(errorHandler);

  return app;
}
