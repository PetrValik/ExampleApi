import type { Article } from '@prisma/client';

/**
 * The article wire shape (snake_case field names, exactly as the contract pins them).
 * `row_version` is the integer optimistic-concurrency token (the `version` column).
 */
export interface ArticleResponse {
  article_id: number;
  name: string;
  description: string;
  category: string | null;
  price: number;
  currency: string | null;
  row_version: number;
}

export function toArticleResponse(article: Article): ArticleResponse {
  return {
    article_id: article.articleId,
    name: article.name,
    description: article.description,
    category: article.category ?? null,
    price: article.price,
    currency: article.currency ?? null,
    row_version: article.version,
  };
}
