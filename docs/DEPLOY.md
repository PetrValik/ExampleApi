# Deploying the showcase

Three complementary paths — pick per what you want public and what you want live. The local Docker
daemon is **not** needed for any of the GitHub-hosted paths (they build on GitHub's runners).

## What runs where (recommended mix)

| Goal | Where | Cost | Live API? |
|------|-------|------|-----------|
| Prove all 10 conform | **GitHub Actions CI** | free | yes (in CI, ephemeral) |
| Public reference page with charts | **GitHub Pages** | free | no (charts only) |
| A live API endpoint + charts | **Render** (reference API + Postgres + dashboard) | free tier | yes (1 impl) |
| The full live switcher (all 10 + Swagger) | **self-host** `docker-compose.all.yml` | your hardware | yes (all 10) |

### Resource math (for self-hosting the full showcase)

Idle: ~10× postgres-alpine (~300 MB) + 5× .NET (~0.8 GB) + 3× Python (~0.3 GB) + 2× Node (~0.2 GB)
+ dashboard ≈ **1.7–2.5 GB total**. Comfortable on a 16 GB box. A shared Postgres process (one
container, ten databases) would save ~270 MB — not worth rewriting every connection string for.
A single shared *database* is **not** possible: the concurrency schemas differ (`.NET` uses the
`xmin` system column, Python/Node use a `version` column), so the tables aren't compatible.

## 1. GitHub Actions CI (the important one)

[`.github/workflows/ci.yml`](../.github/workflows/ci.yml) builds every implementation and runs the
black-box [conformance suite](../conformance) against each — live, on GitHub's runners. This is how
parity gets proven without a working local Docker. It runs on every push and PR; watch it under the
repo's **Actions** tab. Expect it to surface real per-impl gaps on the first run (e.g. a framework's
default validation envelope not yet normalised to `400 problem+json`) — fix them impl by impl.

## 2. GitHub Pages (free public charts)

[`.github/workflows/pages.yml`](../.github/workflows/pages.yml) assembles the dashboard
(`index.html` + `data.json` + `openapi.yaml`) and deploys it to Pages on push to `main`.

One-time: repo **Settings → Pages → Source: GitHub Actions**. The page then lives at
`https://<user>.github.io/<repo>/`. The live API explorer shows charts only there (no backend).

## 3. Render (live reference API + charts)

[`render.yaml`](../render.yaml) is a Blueprint: the reference API (Docker), a managed Postgres, and
the static dashboard. In Render: **New + → Blueprint → connect this repo**. It provisions all three
and auto-deploys on push to `main`.

**One manual step** (Render can't compose it in the blueprint): after the database is created, copy
its **Internal Connection String** and set the API service's `ConnectionStrings__Default` env var in
**Npgsql keyword form**:

```
Host=<internal-host>;Port=5432;Database=exampleapi;Username=<user>;Password=<password>
```

(Render shows the URL form `postgres://user:pass@host/db`; Npgsql needs the keyword form above.)
Redeploy the API service after setting it.

> Free-tier web services sleep when idle and cold-start on the next request — fine for a demo, not
> for benchmarks. Running all ten live on Render would be ~$7/service/month; self-host that instead.

## 4. Self-host the full live showcase

On any box with Docker (your 16 GB machine is plenty):

```bash
docker compose -f docker-compose.all.yml up --build
# dashboard http://localhost:8080 · impls http://localhost:8081..8090
```

This is the only path where the dashboard's **live API explorer** works for all ten, because the
dashboard reverse-proxies to each impl by service name (same-origin, no CORS). Put it behind a
reverse proxy (Caddy/nginx/Cloudflare Tunnel) to expose it.

## Benchmarks (Phase 2)

Wherever the stack runs with Docker, drive the shared load profile and fill the axis-B table:

```bash
BASE_URL=http://localhost:8081 k6 run bench/k6/articles.js   # repeat per impl
```
