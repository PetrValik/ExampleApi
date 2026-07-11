# Contract — the single source of truth

`openapi.yaml` defines the Example API **once**. Every implementation under
`implementations/` must satisfy it exactly, and the [`conformance/`](../conformance)
suite is the executable proof that it does. When the contract and an implementation
disagree, the contract wins — or the contract is bumped deliberately (see *Versioning*).

## What it covers

| Route | Methods | Auth |
|-------|---------|------|
| `/health` | GET | none |
| `/auth/token` | POST | none |
| `/api/articles` | GET, POST | **JWT** |
| `/api/articles/{id}` | GET, PUT, DELETE | **JWT** |
| `/api/articles-concurrent` | POST | **JWT** |

All `/api/articles/**` operations require a bearer token from `POST /auth/token`
(demo credentials `admin` / `admin`).

## Validate it

```bash
python3 -m venv .venv && ./.venv/bin/pip install openapi-spec-validator pyyaml
./.venv/bin/python -c "from openapi_spec_validator import validate; from openapi_spec_validator.readers import read_from_filename; validate(read_from_filename('openapi.yaml')[0]); print('valid')"
```

(The `conformance/.venv` already has this installed.)

## Fixtures

`fixtures/` holds canonical request bodies shared by the conformance suite and docs,
so every implementation is exercised with the identical inputs:

- `credentials.json` — the demo login.
- `valid-article.json` — a well-formed create/update body.
- `invalid-article.json` — a body that must fail validation (empty name, price without currency).

## Known serialization quirk (pinned on purpose)

Article payloads use `snake_case` (`article_id`, `row_version`); the pagination wrapper
and token response use `camelCase` (`pageSize`, `expiresAt`). This mismatch is **documented
and pinned by conformance** rather than silently normalised — it reflects the current
reference implementation. Changing it is a contract change, not an implementation detail.

## Versioning

`info.version` is the contract version. Bump it (and note it in
[`docs/roadmap.md`](../docs/roadmap.md)) whenever a change alters the wire shape — a new
field, a status-code change, a renamed property. Implementations then catch up to the new
version and re-run conformance. Additive, backward-compatible changes bump the patch/minor;
breaking wire changes bump the major.
