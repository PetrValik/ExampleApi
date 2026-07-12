# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ExampleApi/ExampleApi.csproj", "src/ExampleApi/"]
RUN dotnet restore "src/ExampleApi/ExampleApi.csproj"

COPY . .
RUN dotnet publish "src/ExampleApi/ExampleApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ExampleApi.dll"]
