import { Article } from './article.entity';

/** Wire shape of an article (snake_case), including the `row_version` token. */
export interface ArticleResponse {
  article_id: number;
  name: string;
  description: string;
  category: string | null;
  price: number;
  currency: string | null;
  row_version: number;
}

/** camelCase pagination wrapper around snake_case article items. */
export interface PagedArticleResponse {
  items: ArticleResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export function toArticleResponse(article: Article): ArticleResponse {
  return {
    article_id: article.articleId,
    name: article.name,
    description: article.description,
    category: article.category ?? null,
    price:
      typeof article.price === 'string'
        ? parseFloat(article.price)
        : article.price,
    currency: article.currency ?? null,
    row_version: article.version,
  };
}
