import { PrismaClient } from '@prisma/client';

/** A single shared Prisma client for the process. */
export const prisma = new PrismaClient();
