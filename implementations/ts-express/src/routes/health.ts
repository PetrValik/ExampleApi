import { Router } from 'express';

/** GET /health — anonymous liveness probe. */
export const healthRouter: Router = Router();

healthRouter.get('/health', (_req, res) => {
  res.status(200).json({ status: 'healthy' });
});
