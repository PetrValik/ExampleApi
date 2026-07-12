import { Router } from 'express';

import { requireAuth } from '../middleware/auth';
import { asyncHandler } from '../middleware/asyncHandler';
import { toArticleResponse } from '../mappers';
import { prisma } from '../prisma';
import { ConflictError, NotFoundError } from '../problem';
import {
  articleRequestSchema,
  batchCreateSchema,
  parseOrThrow,
  updateArticleRequestSchema,
} from '../validation/schemas';

/**
 * Article resource routes. Every endpoint below requires a valid bearer token —
 * the guard runs for any request that enters this router.
 */
export const articlesRouter: Router = Router();

articlesRouter.use(requireAuth);

const DEFAULT_PAGE = 1;
const DEFAULT_PAGE_SIZE = 10;
const MAX_PAGE_SIZE = 100;

/** Parses a query value to a positive integer, falling back to a default. */
function toPositiveInt(value: unknown, fallback: number): number {
  if (typeof value !== 'string' || value.trim() === '') {
    return fallback;
  }
  const parsed = Number.parseInt(value, 10);
  return Number.isNaN(parsed) ? fallback : parsed;
}

/** Parses an :id path segment; returns null when it is not a valid integer. */
function parseId(raw: string): number | null {
  if (!/^-?\d+$/.test(raw)) {
    return null;
  }
  const id = Number.parseInt(raw, 10);
  return Number.isNaN(id) ? null : id;
}

// GET /api/articles — filter (name partial/ci, category exact) + pagination.
articlesRouter.get(
  '/api/articles',
  asyncHandler(async (req, res) => {
    const nameFilter = typeof req.query.name === 'string' ? req.query.name : undefined;
    const categoryFilter = typeof req.query.category === 'string' ? req.query.category : undefined;

    const page = Math.max(1, toPositiveInt(req.query.page, DEFAULT_PAGE));
    const pageSize = Math.min(
      Math.max(1, toPositiveInt(req.query.pageSize, DEFAULT_PAGE_SIZE)),
      MAX_PAGE_SIZE,
    );

    const where = {
      ...(nameFilter && nameFilter.trim() !== ''
        ? { name: { contains: nameFilter, mode: 'insensitive' as const } }
        : {}),
      ...(categoryFilter && categoryFilter.trim() !== '' ? { category: categoryFilter } : {}),
    };

    const totalCount = await prisma.article.count({ where });
    const articles = await prisma.article.findMany({
      where,
      orderBy: { articleId: 'asc' },
      skip: (page - 1) * pageSize,
      take: pageSize,
    });

    const totalPages = Math.ceil(totalCount / pageSize);

    res.status(200).json({
      items: articles.map(toArticleResponse),
      page,
      pageSize,
      totalCount,
      totalPages,
      hasPrevious: page > 1,
      hasNext: page < totalPages,
    });
  }),
);

// POST /api/articles — create.
articlesRouter.post(
  '/api/articles',
  asyncHandler(async (req, res) => {
    const input = parseOrThrow(articleRequestSchema, req.body);

    const created = await prisma.article.create({
      data: {
        name: input.name,
        description: input.description,
        category: input.category ?? null,
        price: input.price,
        currency: input.currency ?? null,
      },
    });

    res
      .status(201)
      .location(`/api/articles/${created.articleId}`)
      .json(toArticleResponse(created));
  }),
);

// POST /api/articles-concurrent — batch create (order preserved).
articlesRouter.post(
  '/api/articles-concurrent',
  asyncHandler(async (req, res) => {
    const inputs = parseOrThrow(batchCreateSchema, req.body);

    const created = await Promise.all(
      inputs.map((input) =>
        prisma.article.create({
          data: {
            name: input.name,
            description: input.description,
            category: input.category ?? null,
            price: input.price,
            currency: input.currency ?? null,
          },
        }),
      ),
    );

    res.status(201).json(created.map(toArticleResponse));
  }),
);

// GET /api/articles/:id — fetch one.
articlesRouter.get(
  '/api/articles/:id',
  asyncHandler(async (req, res) => {
    const id = parseId(req.params.id);
    if (id === null) {
      throw new NotFoundError(`Article with ID ${req.params.id} was not found.`);
    }

    const article = await prisma.article.findUnique({ where: { articleId: id } });
    if (!article) {
      throw new NotFoundError(`Article with ID ${id} was not found.`);
    }

    res.status(200).json(toArticleResponse(article));
  }),
);

// PUT /api/articles/:id — replace all fields with optimistic concurrency.
articlesRouter.put(
  '/api/articles/:id',
  asyncHandler(async (req, res) => {
    const id = parseId(req.params.id);
    if (id === null) {
      throw new NotFoundError(`Article with ID ${req.params.id} was not found.`);
    }

    const input = parseOrThrow(updateArticleRequestSchema, req.body);

    // 404 if it does not exist at all; 409 only if it exists but the version is stale.
    const existing = await prisma.article.findUnique({ where: { articleId: id } });
    if (!existing) {
      throw new NotFoundError(`Article with ID ${id} was not found.`);
    }

    // Conditional update on (id, version): matches zero rows when the client's
    // row_version is stale, which we surface as 409 Conflict.
    const result = await prisma.article.updateMany({
      where: { articleId: id, version: input.row_version },
      data: {
        name: input.name,
        description: input.description,
        category: input.category ?? null,
        price: input.price,
        currency: input.currency ?? null,
        version: { increment: 1 },
      },
    });

    if (result.count === 0) {
      throw new ConflictError(
        `Article with ID ${id} was modified by another request. Please retry.`,
      );
    }

    const updated = await prisma.article.findUnique({ where: { articleId: id } });
    res.status(200).json(toArticleResponse(updated!));
  }),
);

// DELETE /api/articles/:id — remove.
articlesRouter.delete(
  '/api/articles/:id',
  asyncHandler(async (req, res) => {
    const id = parseId(req.params.id);
    if (id === null) {
      throw new NotFoundError(`Article with ID ${req.params.id} was not found.`);
    }

    const existing = await prisma.article.findUnique({ where: { articleId: id } });
    if (!existing) {
      throw new NotFoundError(`Article with ID ${id} was not found.`);
    }

    await prisma.article.delete({ where: { articleId: id } });
    res.status(204).end();
  }),
);
