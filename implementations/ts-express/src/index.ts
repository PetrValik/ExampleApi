import { createApp } from './app';
import { config } from './config';
import { prisma } from './prisma';

/**
 * Process entry point. The database schema is created/synced by the container
 * entrypoint (`prisma db push`) before this starts, so here we just verify
 * connectivity and begin serving on port 8080.
 */
async function main(): Promise<void> {
  // Fail fast if the database is unreachable at startup.
  await prisma.$connect();

  const app = createApp();
  const server = app.listen(config.port, () => {
    // eslint-disable-next-line no-console
    console.log(`Example API listening on port ${config.port}`);
  });

  const shutdown = async (signal: string): Promise<void> => {
    // eslint-disable-next-line no-console
    console.log(`Received ${signal}, shutting down...`);
    server.close();
    await prisma.$disconnect();
    process.exit(0);
  };

  process.on('SIGTERM', () => void shutdown('SIGTERM'));
  process.on('SIGINT', () => void shutdown('SIGINT'));
}

main().catch(async (error) => {
  // eslint-disable-next-line no-console
  console.error('Fatal startup error:', error);
  await prisma.$disconnect().catch(() => undefined);
  process.exit(1);
});
