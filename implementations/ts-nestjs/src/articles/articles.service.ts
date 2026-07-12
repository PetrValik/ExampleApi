import {
  ConflictException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { plainToInstance } from 'class-transformer';
import { validate } from 'class-validator';
import { Repository } from 'typeorm';
import { ValidationException } from '../common/exceptions/validation.exception';
import { flattenValidationErrors } from '../common/validation-errors';
import { Article } from './article.entity';
import {
  ArticleResponse,
  PagedArticleResponse,
  toArticleResponse,
} from './article.mapper';
import { CreateArticleDto } from './dto/create-article.dto';
import { UpdateArticleDto } from './dto/update-article.dto';

const MAX_PAGE_SIZE = 100;
const DEFAULT_PAGE_SIZE = 10;
const MAX_BATCH_SIZE = 100;

export interface ListArticlesQuery {
  name?: string;
  category?: string;
  page?: string;
  pageSize?: string;
}

@Injectable()
export class ArticlesService {
  constructor(
    @InjectRepository(Article)
    private readonly repository: Repository<Article>,
  ) {}

  async list(query: ListArticlesQuery): Promise<PagedArticleResponse> {
    const page = Math.max(1, parseIntOr(query.page, 1));
    const pageSize = Math.min(
      Math.max(1, parseIntOr(query.pageSize, DEFAULT_PAGE_SIZE)),
      MAX_PAGE_SIZE,
    );

    const qb = this.repository.createQueryBuilder('a');

    if (query.name && query.name.trim().length > 0) {
      qb.andWhere('a.name ILIKE :name', {
        name: `%${escapeLikePattern(query.name)}%`,
      });
    }

    if (query.category && query.category.length > 0) {
      qb.andWhere('a.category = :category', { category: query.category });
    }

    const totalCount = await qb.getCount();

    const articles = await qb
      .orderBy('a.article_id', 'ASC')
      .skip((page - 1) * pageSize)
      .take(pageSize)
      .getMany();

    const totalPages = Math.ceil(totalCount / pageSize);

    return {
      items: articles.map(toArticleResponse),
      page,
      pageSize,
      totalCount,
      totalPages,
      hasPrevious: page > 1,
      hasNext: page < totalPages,
    };
  }

  async getById(id: number): Promise<ArticleResponse> {
    const article = await this.repository.findOne({
      where: { articleId: id },
    });

    if (!article) {
      throw new NotFoundException(`Article with ID ${id} was not found.`);
    }

    return toArticleResponse(article);
  }

  async create(dto: CreateArticleDto): Promise<ArticleResponse> {
    const article = this.repository.create(this.toEntity(dto));
    const saved = await this.repository.save(article);
    return toArticleResponse(saved);
  }

  async update(id: number, dto: UpdateArticleDto): Promise<ArticleResponse> {
    const existing = await this.repository.findOne({
      where: { articleId: id },
    });

    if (!existing) {
      throw new NotFoundException(`Article with ID ${id} was not found.`);
    }

    // Conditional, atomic version check + increment. If the stored version no
    // longer equals the client's row_version, zero rows are affected -> 409.
    const result = await this.repository
      .createQueryBuilder()
      .update(Article)
      .set({
        name: dto.name,
        description: dto.description,
        category: dto.category ?? null,
        price: dto.price,
        currency: dto.price > 0 ? (dto.currency ?? null) : null,
        version: () => 'version + 1',
      })
      .where('article_id = :id', { id })
      .andWhere('version = :version', { version: dto.row_version })
      .execute();

    if (!result.affected || result.affected === 0) {
      throw new ConflictException(
        `Article with ID ${id} was modified by another request. Please retry.`,
      );
    }

    const updated = await this.repository.findOneByOrFail({ articleId: id });
    return toArticleResponse(updated);
  }

  async remove(id: number): Promise<void> {
    const result = await this.repository.delete({ articleId: id });

    if (!result.affected || result.affected === 0) {
      throw new NotFoundException(`Article with ID ${id} was not found.`);
    }
  }

  async batchCreate(body: unknown): Promise<ArticleResponse[]> {
    if (!Array.isArray(body)) {
      throw new ValidationException({
        items: ['Request body must be an array of articles.'],
      });
    }

    if (body.length === 0) {
      throw new ValidationException({
        items: ['At least one article is required.'],
      });
    }

    if (body.length > MAX_BATCH_SIZE) {
      throw new ValidationException({
        items: [`Cannot create more than ${MAX_BATCH_SIZE} articles at once.`],
      });
    }

    const errors: Record<string, string[]> = {};
    const dtos: CreateArticleDto[] = [];

    for (let index = 0; index < body.length; index++) {
      const dto = plainToInstance(CreateArticleDto, body[index]);
      const itemErrors = await validate(dto, { forbidUnknownValues: false });
      if (itemErrors.length > 0) {
        Object.assign(
          errors,
          flattenValidationErrors(itemErrors, `[${index}]`),
        );
      }
      dtos.push(dto);
    }

    if (Object.keys(errors).length > 0) {
      throw new ValidationException(errors);
    }

    const entities = dtos.map((dto) =>
      this.repository.create(this.toEntity(dto)),
    );
    const saved = await this.repository.save(entities);
    return saved.map(toArticleResponse);
  }

  private toEntity(dto: CreateArticleDto): Partial<Article> {
    return {
      name: dto.name,
      description: dto.description,
      category: dto.category ?? null,
      price: dto.price,
      currency: dto.price > 0 ? (dto.currency ?? null) : null,
      version: 1,
    };
  }
}

function parseIntOr(value: string | undefined, fallback: number): number {
  if (value === undefined || value === null || value === '') {
    return fallback;
  }
  const parsed = Number.parseInt(value, 10);
  return Number.isNaN(parsed) ? fallback : parsed;
}

/**
 * Escapes LIKE/ILIKE wildcards so user input is matched literally. Postgres
 * treats backslash as the default escape character, so `\%` and `\_` match the
 * literal characters.
 */
function escapeLikePattern(pattern: string): string {
  return pattern
    .replace(/\\/g, '\\\\')
    .replace(/%/g, '\\%')
    .replace(/_/g, '\\_');
}
