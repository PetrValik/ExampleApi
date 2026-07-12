import 'reflect-metadata';
import { ValidationPipe } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';
import { ValidationError } from 'class-validator';
import { AppModule } from './app.module';
import { ValidationException } from './common/exceptions/validation.exception';
import { AllExceptionsFilter } from './common/filters/all-exceptions.filter';
import { flattenValidationErrors } from './common/validation-errors';

async function bootstrap(): Promise<void> {
  const app = await NestFactory.create(AppModule);

  // Turn class-validator failures into our RFC 7807 problem+json shape instead
  // of Nest's default 400 envelope.
  app.useGlobalPipes(
    new ValidationPipe({
      transform: true,
      whitelist: false,
      forbidUnknownValues: false,
      exceptionFactory: (errors: ValidationError[]) =>
        new ValidationException(flattenValidationErrors(errors)),
    }),
  );

  app.useGlobalFilters(new AllExceptionsFilter());

  const port = Number(process.env.PORT ?? 8080);
  await app.listen(port, '0.0.0.0');
}

void bootstrap();
