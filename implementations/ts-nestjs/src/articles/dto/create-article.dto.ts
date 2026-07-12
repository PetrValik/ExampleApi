import {
  IsIn,
  IsNotEmpty,
  IsNumber,
  IsOptional,
  IsString,
  Max,
  MaxLength,
  Min,
  ValidateIf,
} from 'class-validator';
import { SUPPORTED_CURRENCIES } from '../../common/currencies';

/**
 * Body for POST /api/articles and each item of POST /api/articles-concurrent.
 * Field names are snake_case per the wire contract; property names already
 * match, so no aliasing is required.
 */
export class CreateArticleDto {
  @IsString({ message: 'Name must be a string.' })
  @IsNotEmpty({ message: 'Name is required.' })
  @MaxLength(64, { message: 'Name must not exceed 64 characters.' })
  name!: string;

  @IsString({ message: 'Description must be a string.' })
  @IsNotEmpty({ message: 'Description is required.' })
  @MaxLength(2048, { message: 'Description must not exceed 2048 characters.' })
  description!: string;

  @IsOptional()
  @IsString({ message: 'Category must be a string.' })
  @MaxLength(64, { message: 'Category must not exceed 64 characters.' })
  category?: string | null;

  @IsNumber({}, { message: 'Price must be a number.' })
  @Min(0, { message: 'Price must be greater than or equal to 0.' })
  @Max(9999999999999999.99, {
    message: 'Price must not exceed 9999999999999999.99.',
  })
  price!: number;

  // Required and validated only when price > 0; ignored for free articles.
  @ValidateIf((o: CreateArticleDto) => o.price > 0)
  @IsString({ message: 'Currency is required when price is greater than 0.' })
  @IsIn(SUPPORTED_CURRENCIES, {
    message: 'Currency must be a supported ISO 4217 code.',
  })
  currency?: string | null;
}
