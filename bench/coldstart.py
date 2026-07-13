#!/usr/bin/env python3
"""Cold-start benchmark: time from `docker compose up` to the first healthy response.

Images are pre-built (untimed) so we measure container start + DB wait + app boot + first-request
migration — the real "how fast does the service come up" cost. Median of 3 per implementation.
Includes a constant ~DB-start offset (same postgres image everywhere), so read it as a comparison.

    python3 bench/coldstart.py > bench/coldstart.json
"""
import json
import subprocess
import tempfile
import time
import urllib.request
from datetime import datetime, timezone
from pathlib import Path
from statistics import median

ROOT = Path(__file__).resolve().parent.parent
IMPLS = [
    "dotnet-vsa", "dotnet-minimal", "dotnet-mvc", "dotnet-clean", "dotnet-mediatr",
    "python-fastapi", "python-django", "python-flask", "ts-express", "ts-nestjs",
]
RUNS = 3
BASE = "http://localhost:8080"


def sh(cmd: str, cwd: Path) -> None:
    subprocess.run(cmd, cwd=cwd, shell=True, check=False,
                   stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)


def out(cmd: str, cwd: Path) -> str:
    r = subprocess.run(cmd, cwd=cwd, shell=True, check=False,
                       capture_output=True, text=True)
    return r.stdout.strip()


def db_service(cwd: Path) -> str:
    """The non-api service in the impl's compose is its database (usually `postgres`, `db` for flask)."""
    svcs = [s for s in out("docker compose config --services", cwd).splitlines() if s and s != "api"]
    return svcs[0] if svcs else "postgres"


def fast_healthcheck_override(db: str) -> Path:
    """A compose override that makes the DB healthcheck fire fast, so the api's depends_on gate clears
    in ~1s instead of the default 5s interval — isolating the app's own startup cost."""
    fd = tempfile.NamedTemporaryFile("w", suffix=".yml", delete=False)
    fd.write(f"services:\n  {db}:\n    healthcheck:\n"
             "      interval: 1s\n      timeout: 2s\n      retries: 30\n      start_period: 0s\n")
    fd.close()
    return Path(fd.name)


def time_to_health(timeout: float = 120.0):
    start = time.time()
    while time.time() - start < timeout:
        try:
            with urllib.request.urlopen(f"{BASE}/health", timeout=1) as r:
                if r.status == 200:
                    return round(time.time() - start, 2)
        except Exception:
            pass
        time.sleep(0.15)
    return None


results = {}
for impl in IMPLS:
    d = ROOT / "implementations" / impl
    db = db_service(d)
    ov = fast_healthcheck_override(db)
    base = f"docker compose -f docker-compose.yml -f {ov}"
    sh(f"{base} build", d)                  # build untimed
    sh(f"{base} down -v", d)                # ensure clean slate
    times = []
    for _ in range(RUNS):
        t0 = time.time()
        sh(f"{base} up -d", d)             # images cached, fast DB healthcheck → app startup dominates
        elapsed = time_to_health()
        if elapsed is not None:
            times.append(round(time.time() - t0, 2))
        sh(f"{base} down -v", d)
    ov.unlink(missing_ok=True)
    if times:
        results[impl] = {"coldStartSec": round(median(times), 2), "runs": times}
    print(f"# {impl}: {results.get(impl)}", flush=True)

out = {
    "ranAt": datetime.now(timezone.utc).isoformat(timespec="seconds"),
    "runner": "local cold start — median of 3, images pre-built, fast DB healthcheck (isolates app startup)",
    "results": results,
}
Path(ROOT / "bench" / "coldstart.json").write_text(json.dumps(out, indent=2) + "\n")
print("wrote bench/coldstart.json")
