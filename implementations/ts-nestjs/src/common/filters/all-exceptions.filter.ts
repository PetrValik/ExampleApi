import {
  ArgumentsHost,
  Catch,
  ExceptionFilter,
  HttpException,
  Logger,
} from '@nestjs/common';
import { Response } from 'express';
import { ValidationException } from '../exceptions/validation.exception';

/**
 * Global exception filter that normalises every error response to
 * RFC 7807 `application/problem+json`.
 *
 * - {@link ValidationException} -> 400 with an `errors` map (never the
 *   framework's default validation envelope).
 * - Any {@link HttpException} (401/404/409/...) -> problem+json with
 *   `type`/`title`/`status`/`detail`.
 * - Anything else -> 500 problem+json.
 */
@Catch()
export class AllExceptionsFilter implements ExceptionFilter {
  private readonly logger = new Logger(AllExceptionsFilter.name);

  catch(exception: unknown, host: ArgumentsHost): void {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();

    let status: number;
    let body: Record<string, unknown>;

    if (exception instanceof ValidationException) {
      status = 400;
      body = {
        type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
        title: 'One or more validation errors occurred.',
        status,
        errors: exception.errors,
      };
    } else if (exception instanceof HttpException) {
      status = exception.getStatus();
      body = {
        type: `https://httpstatuses.com/${status}`,
        title: titleForStatus(status),
        status,
        detail: extractDetail(exception),
      };
    } else {
      status = 500;
      this.logger.error(
        'Unhandled exception',
        exception instanceof Error ? exception.stack : String(exception),
      );
      body = {
        type: 'https://httpstatuses.com/500',
        title: 'Internal Server Error',
        status,
        detail: 'An unexpected error occurred.',
      };
    }

    response.setHeader('Content-Type', 'application/problem+json');
    response.status(status).json(body);
  }
}

function titleForStatus(status: number): string {
  switch (status) {
    case 400:
      return 'Bad Request';
    case 401:
      return 'Unauthorized';
    case 403:
      return 'Forbidden';
    case 404:
      return 'Not Found';
    case 409:
      return 'Conflict';
    default:
      return status >= 500 ? 'Internal Server Error' : 'Error';
  }
}

function extractDetail(exception: HttpException): string {
  const response = exception.getResponse();

  if (typeof response === 'string') {
    return response;
  }

  if (response && typeof response === 'object') {
    const message = (response as { message?: unknown }).message;
    if (Array.isArray(message)) {
      return message.join(', ');
    }
    if (typeof message === 'string') {
      return message;
    }
  }

  return exception.message;
}
