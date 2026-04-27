FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/backend/HomeFinder.Api.csproj", "src/backend/"]
RUN dotnet restore "src/backend/HomeFinder.Api.csproj"

COPY . .
RUN dotnet publish "src/backend/HomeFinder.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HomeFinder.Api.dll"]
