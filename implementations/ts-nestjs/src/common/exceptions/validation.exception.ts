/**
 * Carries a per-field validation error map so the global exception filter can
 * render it as HTTP 400 `application/problem+json` with an `errors` object.
 *
 * This deliberately does NOT extend Nest's `HttpException` — the filter special
 * cases it to reshape the framework's default validation envelope into RFC 7807.
 */
export class ValidationException extends Error {
  constructor(public readonly errors: Record<string, string[]>) {
    super('One or more validation errors occurred.');
    this.name = 'ValidationException';
  }
}
