# syntax=docker/dockerfile:1

# ---- Build stage: install all deps, generate the Prisma client, compile TS ----
# Debian (glibc) rather than Alpine (musl): Prisma's query/schema engines need libssl,
# which is fragile on Alpine (the engine fails to load and `prisma db push` errors out).
FROM node:20-bookworm-slim AS build
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends openssl \
    && rm -rf /var/lib/apt/lists/*

COPY package.json package-lock.json* ./
RUN npm install

COPY prisma ./prisma
RUN npx prisma generate

COPY tsconfig.json ./
COPY src ./src
RUN npm run build

# ---- Runtime stage ----
FROM node:20-bookworm-slim AS final
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=8080

# openssl is required by the Prisma engine that runs `prisma db push` at startup.
RUN apt-get update && apt-get install -y --no-install-recommends openssl \
    && rm -rf /var/lib/apt/lists/*

# Bring over the fully-installed node_modules (includes the Prisma CLI needed for
# `prisma db push` at startup and the generated client) plus the build output.
COPY --from=build /app/node_modules ./node_modules
COPY --from=build /app/dist ./dist
COPY package.json ./
COPY prisma ./prisma
COPY docker-entrypoint.sh ./
RUN chmod +x docker-entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["./docker-entrypoint.sh"]
