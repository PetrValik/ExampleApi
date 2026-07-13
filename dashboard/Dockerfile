# Static comparison dashboard + reverse proxy to the implementations.
# Build from the REPO ROOT so it can copy the shared contract:
#   docker build -f dashboard/Dockerfile -t example-api-dashboard .
FROM nginx:1.27-alpine

COPY dashboard/nginx.conf /etc/nginx/conf.d/default.conf
COPY dashboard/index.html /usr/share/nginx/html/index.html
COPY dashboard/data.json  /usr/share/nginx/html/data.json
COPY contract/openapi.yaml /usr/share/nginx/html/openapi.yaml
# Optional benchmark result files (present after running the bench scripts).
COPY bench/ /tmp/bench/
RUN for j in sweep coldstart profiles workers; do cp "/tmp/bench/$j.json" "/usr/share/nginx/html/$j.json" 2>/dev/null || true; done; rm -rf /tmp/bench

EXPOSE 80
