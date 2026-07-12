# Example API — a multi-approach showcase

> The **same** small API — article/product CRUD, JWT auth, health — implemented many ways,
> so the approaches can be compared **fairly**, side by side.

One OpenAPI contract. One conformance suite every implementation must pass. One benchmark
harness. Different architectures on the same runtime; different runtimes entirely. The name
"Example API" taken literally.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL 16](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

## Why

A CRUD API is small enough to implement repeatedly, yet rich enough to expose the real
differences between approaches. Implementing it once per architecture/runtime and holding
everything else constant (same contract, same database, same load) turns vague opinions
("VSA is simpler", "FastAPI is faster") into numbers you can point at.

### Two comparison axes — kept separate on purpose

Mixing these is the trap that makes benchmark numbers meaningless:

| Axis | Members | What actually differs | What we measure |
|------|---------|-----------------------|-----------------|
| **A · same runtime, different architecture** | .NET: VSA, MVC, Clean Arch, MediatR | code shape / ceremony — **not** speed (all ASP.NET Core → near-identical throughput) | files, LOC, coupling, testability |
| **B · different runtime / framework** | .NET vs FastAPI vs Django vs Express | the runtime itself | RPS, p50/p95/p99, RAM, image size, cold start |

"VSA is 2 % faster than MediatR" is noise from GC/JIT. "VSA needs N fewer files than MVC"
(axis A) and "FastAPI serves X RPS vs .NET's Y" (axis B) are the real stories. See
[`docs/comparison.md`](docs/comparison.md).

## Layout

```
example-api/
├── contract/           # openapi.yaml — the single source of truth (+ fixtures)
├── conformance/        # black-box HTTP suite every implementation must pass
├── bench/              # load-test methodology + k6 harness
├── implementations/
│   └── dotnet-vsa/     # ← the reference implementation (.NET Vertical Slice)
├── docs/               # comparison tables, roadmap, design specs
└── scripts/            # verify-impl.sh, metrics.sh
```

## Implementations

All ten build/typecheck clean and ship a Dockerfile + compose (own PostgreSQL, port 8080).
"Conforms" = passes the [conformance suite](conformance); it is *by construction* until the live
run executes (pending Docker — see *Status*).

| Approach | Runtime | Axis | Build | Conforms |
|----------|---------|:----:|:-----:|:--------:|
| [**dotnet-vsa**](implementations/dotnet-vsa) — Vertical Slice *(reference)* | .NET 10 | A | ✅ | suite ready¹ |
| [dotnet-minimal](implementations/dotnet-minimal) — one file, no abstractions | .NET 10 | A | ✅ | by-construction |
| [dotnet-mvc](implementations/dotnet-mvc) — Controllers/Services/Repos | .NET 10 | A | ✅ | by-construction |
| [dotnet-clean](implementations/dotnet-clean) — Clean Architecture (4 projects) | .NET 10 | A | ✅ | by-construction |
| [dotnet-mediatr](implementations/dotnet-mediatr) — VSA + MediatR + Result | .NET 10 | A | ✅ | by-construction |
| [python-fastapi](implementations/python-fastapi) — FastAPI + SQLAlchemy | Python | B | ✅ | by-construction |
| [python-django](implementations/python-django) — Django + DRF | Python | B | ✅ | by-construction |
| [python-flask](implementations/python-flask) — Flask + SQLAlchemy | Python | B | ✅ | by-construction |
| [ts-express](implementations/ts-express) — Express + Prisma | Node/TS | B | ✅ | by-construction |
| [ts-nestjs](implementations/ts-nestjs) — NestJS + TypeORM | Node/TS | B | ✅ | by-construction |

¹ The conformance suite is complete and statically verified; every impl is built to it. The live
green run is pending a Docker daemon (see *Status*). Contract details every impl follows:
[`implementations/CONTRACT-FOR-IMPLEMENTERS.md`](implementations/CONTRACT-FOR-IMPLEMENTERS.md).

## How it fits together

```
contract/openapi.yaml ──► every implementation implements it
        │
        ├──► conformance/  drives each impl over HTTP, proves behavioural parity
        └──► bench/         loads each impl identically, collects the metrics
                                   │
                                   ▼
                            docs/comparison.md  (the tables)
```

An implementation is "done" when it **passes conformance**. Only then do its metrics go in a
table — otherwise you would be comparing apps that don't do the same thing.

## Quickstart

Run the reference implementation and prove it conforms (needs Docker for PostgreSQL):

```bash
# boot the reference impl + its database
cd implementations/dotnet-vsa && docker compose up --build -d && cd -

# run the black-box conformance suite against it
./conformance/run.sh http://localhost:8080
```

Or end to end (boot → wait for health → conformance → teardown):

```bash
./scripts/verify-impl.sh implementations/dotnet-vsa
```

Code-size metrics for the axis-A table:

```bash
./scripts/metrics.sh
```

## Dashboard

A single-page control panel ([`dashboard/`](dashboard)) is the visual front door: pick an
implementation, inspect its stack and code metrics, **call it live** through an embedded Swagger UI,
and read the code-ceremony charts. It reads the same metrics the tables do.

```bash
# the whole showcase — dashboard + all ten implementations, each with its own database
docker compose -f docker-compose.all.yml up --build
# dashboard http://localhost:8080  ·  implementations http://localhost:8081..8090
```

Charts and switcher work standalone; the live API explorer needs the implementations running.
The dashboard proxies to each impl by service name (`/impl/<name>/`), so Swagger's "try it out"
stays same-origin. See [`dashboard/README.md`](dashboard/README.md).

## Status

**Phase 0 complete + all 10 implementations built.** The reference (`dotnet-vsa`) is polished, the
contract + conformance suite are extracted, and **nine sibling implementations** now exist — each
building/typechecking clean and Docker-ready. See [`docs/comparison.md`](docs/comparison.md) for the
full numbers, [`docs/roadmap.md`](docs/roadmap.md) for what's next, and
[`docs/superpowers/specs/`](docs/superpowers/specs) for the design.

**First finding (axis A — code ceremony).** The same behaviour, across the .NET styles, spans
**1 file / 382 lines** (`dotnet-minimal`, everything inline) to **48 files / 1,823 lines**
(`dotnet-vsa`) — a ~48× file-count and ~4.8× line spread with near-identical runtime performance.
Layered and MediatR styles land in between (29–46 files). The takeaway isn't "minimal wins":
structure buys boundaries, testability and delete-a-folder-to-delete-a-feature — this just makes
the price of that structure visible. Cross-runtime, the Python trio (11–16 files) and Express
(14) are markedly leaner than any structured .NET style; NestJS (21) recreates the ceremony trade
inside Node. Performance (axis B) is the real cross-runtime question and lands in Phase 2.

> **Docker was unavailable in the build environment**, so the live conformance runs and benchmarks
> have **not executed locally** — every implementation is built *to* the contract. The
> [CI workflow](.github/workflows/ci.yml) runs the conformance suite against all ten on GitHub's
> runners (no local Docker needed), so the green run happens on push. Treat "conforms" as
> by-construction until CI is green.

## Deploy

Cloud paths that don't need a local Docker daemon — CI (proves parity on runners), GitHub Pages
(free public charts), Render (live reference API + Postgres), and self-hosting the full live
switcher. See [`docs/DEPLOY.md`](docs/DEPLOY.md).

## License

[MIT](LICENSE).
