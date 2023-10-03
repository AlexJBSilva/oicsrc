FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a debug version
#RUN dotnet publish -c Debug -o out

# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /App
COPY --from=build-env /App/out .
COPY --from=build-env /App/var/rinha /var/rinha
ENTRYPOINT ["dotnet", "InterpretadorDaRinha.dll"]
