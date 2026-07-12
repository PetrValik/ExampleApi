import { ValidationError } from 'class-validator';

/**
 * Flattens a nested class-validator {@link ValidationError} tree into an
 * RFC 7807 `errors` map: `{ fieldName: ["message", ...] }`.
 *
 * A `prefix` (e.g. `"[0]"`) is used by the batch endpoint so that per-item
 * failures are reported as `"[0].name"`, `"[2].currency"`, etc.
 */
export function flattenValidationErrors(
  errors: ValidationError[],
  prefix = '',
): Record<string, string[]> {
  const result: Record<string, string[]> = {};

  for (const error of errors) {
    const key = prefix ? `${prefix}.${error.property}` : error.property;

    if (error.constraints) {
      result[key] = Object.values(error.constraints);
    }

    if (error.children && error.children.length > 0) {
      Object.assign(result, flattenValidationErrors(error.children, key));
    }
  }

  return result;
}
