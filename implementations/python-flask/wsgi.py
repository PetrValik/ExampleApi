"""Gunicorn / WSGI entrypoint.

``gunicorn wsgi:app`` imports this module; ``init_db`` creates the schema at
startup (retrying while Postgres warms up behind the compose healthcheck).
"""

from app import create_app, init_db

app = create_app()
init_db(app)


if __name__ == "__main__":
    # Local dev convenience; production uses gunicorn (see Dockerfile).
    app.run(host="0.0.0.0", port=8080)
