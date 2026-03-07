# Stage 1: The Base Image (contains the ASP.NET 9 Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: The Build Environment (contains the massive .NET 9 SDK)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy just the csproj file first (this caches dependencies to make builds faster)
COPY ["DigitalMenuApi/DigitalMenuApi.csproj", "DigitalMenuApi/"]
RUN dotnet restore "DigitalMenuApi/DigitalMenuApi.csproj"

# Copy the rest of the code and build it
COPY . .
WORKDIR "/src/DigitalMenuApi"
RUN dotnet build "DigitalMenuApi.csproj" -c Release -o /app/build

# Stage 3: Publish the compiled code
FROM build AS publish
RUN dotnet publish "DigitalMenuApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: The Final Image (Throw away the SDK, keep only the small Runtime and the App)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define the command to start the app
ENTRYPOINT ["dotnet", "DigitalMenuApi.dll"]
