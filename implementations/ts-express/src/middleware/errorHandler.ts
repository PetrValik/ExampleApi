import type { NextFunction, Request, Response } from 'express';

import { ApiError, ValidationProblem, problemBody } from '../problem';

const PROBLEM_JSON = 'application/problem+json';

/**
 * Handles malformed JSON bodies (thrown by express.json() before any route runs)
 * and normalises them to a 400 application/problem+json, never Express's default
 * HTML error page.
 */
export function jsonSyntaxErrorHandler(
  err: unknown,
  _req: Request,
  res: Response,
  next: NextFunction,
): void {
  if (err instanceof SyntaxError && 'body' in (err as SyntaxError & { body?: unknown })) {
    res
      .status(400)
      .type(PROBLEM_JSON)
      .json(problemBody(400, 'One or more validation errors occurred.', 'The request body is not valid JSON.'));
    return;
  }
  next(err);
}

/**
 * Central error handler: turns the API's typed errors into RFC 7807 responses and
 * anything unexpected into a 500 problem+json. This is the single place validation,
 * not-found and conflict errors become HTTP responses.
 */
export function errorHandler(
  err: unknown,
  _req: Request,
  res: Response,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  _next: NextFunction,
): void {
  if (err instanceof ValidationProblem) {
    res
      .status(err.status)
      .type(PROBLEM_JSON)
      .json(problemBody(err.status, err.title, undefined, err.errors));
    return;
  }

  if (err instanceof ApiError) {
    res
      .status(err.status)
      .type(PROBLEM_JSON)
      .json(problemBody(err.status, err.title, err.message));
    return;
  }

  // Unexpected — log and return a generic 500 problem+json.
  console.error('Unhandled error:', err);
  res
    .status(500)
    .type(PROBLEM_JSON)
    .json(problemBody(500, 'Internal Server Error', 'An unexpected error occurred.'));
}
