/**
 * RFC 7807 problem-details helpers and the typed errors the API throws.
 *
 * Every error surface in this API normalises to `application/problem+json`:
 *   - validation failures -> 400 with an `errors` map (field name -> messages)
 *   - not-found           -> 404 problem+json
 *   - concurrency conflict -> 409 problem+json
 * The central error handler (middleware/errorHandler.ts) turns these into responses.
 */

export type ValidationErrors = Record<string, string[]>;

/** Base class for errors that carry an HTTP status and an RFC 7807 body. */
export abstract class ApiError extends Error {
  abstract readonly status: number;
  abstract readonly title: string;
}

/** 400 — request failed validation; carries per-field messages. */
export class ValidationProblem extends ApiError {
  readonly status = 400;
  readonly title = 'One or more validation errors occurred.';
  readonly errors: ValidationErrors;

  constructor(errors: ValidationErrors) {
    super('Validation failed');
    this.errors = errors;
  }
}

/** 404 — the requested resource does not exist. */
export class NotFoundError extends ApiError {
  readonly status = 404;
  readonly title = 'Not Found';

  constructor(detail: string) {
    super(detail);
  }
}

/** 409 — optimistic-concurrency conflict; the row was modified concurrently. */
export class ConflictError extends ApiError {
  readonly status = 409;
  readonly title = 'Conflict';

  constructor(detail: string) {
    super(detail);
  }
}

export interface ProblemDetailsBody {
  type: string;
  title: string;
  status: number;
  detail?: string | null;
  errors?: ValidationErrors;
}

/** Builds an RFC 7807 problem-details body. */
export function problemBody(
  status: number,
  title: string,
  detail?: string | null,
  errors?: ValidationErrors,
): ProblemDetailsBody {
  const body: ProblemDetailsBody = {
    type: `https://httpstatuses.com/${status}`,
    title,
    status,
  };
  if (detail !== undefined && detail !== null) {
    body.detail = detail;
  }
  if (errors !== undefined) {
    body.errors = errors;
  }
  return body;
}
