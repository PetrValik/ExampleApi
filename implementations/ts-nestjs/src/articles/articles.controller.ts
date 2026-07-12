import {
  Body,
  Controller,
  Delete,
  Get,
  HttpCode,
  Param,
  ParseIntPipe,
  Post,
  Put,
  Query,
  Res,
  UseGuards,
} from '@nestjs/common';
import { Response } from 'express';
import { JwtAuthGuard } from '../auth/jwt-auth.guard';
import { ArticlesService, ListArticlesQuery } from './articles.service';
import { CreateArticleDto } from './dto/create-article.dto';
import { UpdateArticleDto } from './dto/update-article.dto';

/**
 * All article endpoints require a valid JWT bearer token.
 */
@UseGuards(JwtAuthGuard)
@Controller('api')
export class ArticlesController {
  constructor(private readonly articlesService: ArticlesService) {}

  @Get('articles')
  list(@Query() query: ListArticlesQuery) {
    return this.articlesService.list(query);
  }

  @Post('articles')
  @HttpCode(201)
  async create(
    @Body() dto: CreateArticleDto,
    @Res({ passthrough: true }) res: Response,
  ) {
    const article = await this.articlesService.create(dto);
    res.setHeader('Location', `/api/articles/${article.article_id}`);
    return article;
  }

  @Get('articles/:id')
  getById(@Param('id', ParseIntPipe) id: number) {
    return this.articlesService.getById(id);
  }

  @Put('articles/:id')
  update(
    @Param('id', ParseIntPipe) id: number,
    @Body() dto: UpdateArticleDto,
  ) {
    return this.articlesService.update(id, dto);
  }

  @Delete('articles/:id')
  @HttpCode(204)
  async remove(@Param('id', ParseIntPipe) id: number): Promise<void> {
    await this.articlesService.remove(id);
  }

  @Post('articles-concurrent')
  @HttpCode(201)
  batchCreate(@Body() body: unknown) {
    return this.articlesService.batchCreate(body);
  }
}
