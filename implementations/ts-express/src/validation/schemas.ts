import { z } from 'zod';

import { isSupportedCurrency } from '../currencies';
import { ValidationProblem, type ValidationErrors } from '../problem';

/**
 * zod schemas for the article payloads plus a helper that runs a schema and, on
 * failure, throws a ValidationProblem (400 application/problem+json) with an
 * `errors` map keyed by field name — never a framework default 422 envelope.
 */

const MAX_PRICE = 9_999_999_999_999_999.99;

/**
 * Applies the price/currency cross-field rule shared by create and update:
 * currency is required, exactly 3 chars, and a supported ISO 4217 code — but
 * only when price > 0. For free articles (price 0) currency is ignored.
 */
function refineCurrency(
  value: { price: number; currency?: string | null },
  ctx: z.RefinementCtx,
): void {
  if (value.price > 0) {
    if (value.currency === null || value.currency === undefined || value.currency === '') {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['currency'],
        message: 'Currency is required when price is greater than 0.',
      });
    } else if (value.currency.length !== 3) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['currency'],
        message: 'Currency must be a valid ISO 4217 code (3 characters).',
      });
    } else if (!isSupportedCurrency(value.currency)) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ['currency'],
        message: 'Currency must be a supported currency code.',
      });
    }
  }
}

const nameField = z
  .string({ required_error: 'Name is required.', invalid_type_error: 'Name is required.' })
  .min(1, 'Name is required.')
  .max(64, 'Name must not exceed 64 characters.');

const descriptionField = z
  .string({
    required_error: 'Description is required.',
    invalid_type_error: 'Description is required.',
  })
  .min(1, 'Description is required.')
  .max(2048, 'Description must not exceed 2048 characters.');

const categoryField = z
  .string()
  .max(64, 'Category must not exceed 64 characters.')
  .nullable()
  .optional();

const priceField = z
  .number({ required_error: 'Price is required.', invalid_type_error: 'Price must be a number.' })
  .min(0, 'Price must be greater than or equal to 0.')
  .max(MAX_PRICE, 'Price must not exceed 9,999,999,999,999,999.99.');

const currencyField = z.string().nullable().optional();

/** POST /api/articles and each item of POST /api/articles-concurrent. */
export const articleRequestSchema = z
  .object({
    name: nameField,
    description: descriptionField,
    category: categoryField,
    price: priceField,
    currency: currencyField,
  })
  .superRefine(refineCurrency);

export type ArticleRequestInput = z.infer<typeof articleRequestSchema>;

/** PUT /api/articles/{id} — adds the required optimistic-concurrency token. */
export const updateArticleRequestSchema = z
  .object({
    name: nameField,
    description: descriptionField,
    category: categoryField,
    price: priceField,
    currency: currencyField,
    row_version: z
      .number({
        required_error: 'Row version is required and must be a valid non-zero value.',
        invalid_type_error: 'Row version is required and must be a valid non-zero value.',
      })
      .int('Row version must be an integer.')
      .min(1, 'Row version is required and must be a valid non-zero value.'),
  })
  .superRefine(refineCurrency);

export type UpdateArticleRequestInput = z.infer<typeof updateArticleRequestSchema>;

/** POST /api/articles-concurrent — a non-empty array of at most 100 create bodies. */
export const batchCreateSchema = z
  .array(articleRequestSchema)
  .min(1, 'At least one article is required.')
  .max(100, 'Cannot create more than 100 articles at once.');

export type BatchCreateInput = z.infer<typeof batchCreateSchema>;

/**
 * Runs a zod schema against arbitrary input. On success returns the parsed value;
 * on failure throws a ValidationProblem carrying a field -> messages map so the
 * central error handler can emit 400 application/problem+json.
 */
export function parseOrThrow<TOutput>(
  schema: z.ZodType<TOutput>,
  input: unknown,
): TOutput {
  const result = schema.safeParse(input);
  if (result.success) {
    return result.data;
  }

  const errors: ValidationErrors = {};
  for (const issue of result.error.issues) {
    const key = issue.path.length > 0 ? issue.path.join('.') : '_';
    (errors[key] ??= []).push(issue.message);
  }
  throw new ValidationProblem(errors);
}
