import { IsDefined, IsInt, Min } from 'class-validator';
import { CreateArticleDto } from './create-article.dto';

/**
 * Body for PUT /api/articles/{id}. Same shape as create plus the required
 * `row_version` optimistic-concurrency token (must be a non-zero integer).
 */
export class UpdateArticleDto extends CreateArticleDto {
  @IsDefined({ message: 'Row version is required.' })
  @IsInt({ message: 'Row version must be an integer.' })
  @Min(1, {
    message: 'Row version is required and must be a valid non-zero value.',
  })
  row_version!: number;
}
