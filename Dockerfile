FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0 AS server-build
WORKDIR /api

# Copy csproj and restore as distinct layers
COPY ./api/OpenSkull.Api/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY ./api/OpenSkull.Api ./
RUN dotnet publish -c Release -o out

FROM --platform=$BUILDPLATFORM node:18.7.0 as webapp-build

WORKDIR /webapp

COPY webapp/package.json webapp/package-lock.json ./
RUN npm ci

COPY webapp .
RUN npm run build

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=api-build /api/out /app
COPY --from=webapp-build /webapp/dist /app/Static
ENTRYPOINT ["dotnet", "OpenSkull.Api.dll"]