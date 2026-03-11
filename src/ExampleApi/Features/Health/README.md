# Health Check Feature

Simple health check endpoint to verify API availability.

## Endpoint

```
GET /health
```

## Response

**Success (200 OK)**

```json
{
  "status": "Healthy"
}
```

## Behavior

- Always returns 200 OK if API is running
- No database connection check (basic liveness probe)
- Useful for load balancers and monitoring systems

## Files

- `HealthEndpoint.cs` - Endpoint definition

## ExampleApi

```bash
curl http://localhost:5088/health
```

## Use Cases

### Docker Health Check

```dockerfile
HEALTHCHECK --interval=30s --timeout=3s \
  CMD curl -f http://localhost:5088/health || exit 1
```

### Kubernetes Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 3
  periodSeconds: 10
```

### Load Balancer

Most load balancers can use this endpoint to check if the instance is healthy before routing traffic.

## Extension Ideas

For production, consider adding:
- Database connectivity check
- External service checks
- Disk space check
- Memory usage check

Use **ASP.NET Core Health Checks** for advanced scenarios:

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();
    
app.MapHealthChecks("/health");
```
