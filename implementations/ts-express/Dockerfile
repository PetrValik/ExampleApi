# syntax=docker/dockerfile:1

# ---- Build stage: install all deps, generate the Prisma client, compile TS ----
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install

COPY prisma ./prisma
RUN npx prisma generate

COPY tsconfig.json ./
COPY src ./src
RUN npm run build

# ---- Runtime stage ----
FROM node:20-alpine AS final
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=8080

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
