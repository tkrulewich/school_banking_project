# Use the official .NET 8.0 runtime image as a base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8.0 SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the main project file and restore dependencies
COPY ["CommerceBankWebApp/CommerceBankWebApp.csproj", "CommerceBankWebApp/"]
RUN dotnet restore "CommerceBankWebApp/CommerceBankWebApp.csproj"

# Copy the main project source code into the container
COPY ./CommerceBankWebApp ./CommerceBankWebApp

# Set the working directory to the main project
WORKDIR /src/CommerceBankWebApp

# Build and publish the main project
RUN dotnet publish -c Release -o /app/publish

# Use the runtime image to run the app
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommerceBankWebApp.dll"]
