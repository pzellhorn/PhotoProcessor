FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
 
COPY Core/ Core/
COPY src/ src/

RUN dotnet restore src/PhotoProcessor/PhotoProcessor.API.csproj
RUN dotnet publish src/PhotoProcessor/PhotoProcessor.API.csproj \
        -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
 
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# `migrate` makes Program.cs run EF migrations for a fresh postgresSQL db and exit
ENTRYPOINT ["dotnet", "PhotoProcessor.API.dll"]
