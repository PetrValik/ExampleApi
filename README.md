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

| Approach | Runtime | Axis | Status | Conformance |
|----------|---------|:----:|--------|:-----------:|
| [**dotnet-vsa**](implementations/dotnet-vsa) — Vertical Slice | .NET 10 | A (reference) | ✅ reference | suite ready¹ |
| dotnet-mvc — Controllers/Services/Repos | .NET 10 | A | 🔲 planned | — |
| dotnet-clean — Clean Architecture | .NET 10 | A | 🔲 planned | — |
| dotnet-mediatr — VSA + MediatR/Result | .NET 10 | A | 🔲 planned | — |
| python-fastapi | Python | B | 🔲 planned | — |
| python-django | Python | B | 🔲 planned | — |
| ts-express | Node/TS | B | 🔲 planned | — |

¹ The conformance suite is complete and statically verified; the live green run is pending a
Docker daemon (see *Status* below).

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

## Status

**Phase 0 complete** — the current .NET code is now the clean reference implementation
(`dotnet-vsa`), the contract and conformance suite are extracted, and the repo is structured
to grow. See [`docs/roadmap.md`](docs/roadmap.md) for what's next and
[`docs/superpowers/specs/`](docs/superpowers/specs) for the design.

> The live conformance/integration/bench runs need a Docker daemon (Testcontainers for the
> .NET integration tests; docker-compose for conformance and bench). Everything is authored
> and statically verified; the runs are one command away where Docker is available.

## License

[MIT](LICENSE).
