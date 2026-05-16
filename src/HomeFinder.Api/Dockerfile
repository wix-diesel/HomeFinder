FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 既存の先頭部分に HomeFinder.Core を追加
COPY ["src/Directory.Build.props", "./"]
COPY ["src/Directory.Packages.props", "./"]
COPY ["src/HomeFinder.Api/HomeFinder.Api.csproj", "HomeFinder.Api/"]
COPY ["src/HomeFinder.Application/HomeFinder.Application.csproj", "HomeFinder.Application/"]
COPY ["src/HomeFinder.Infrastructure/HomeFinder.Infrastructure.csproj", "HomeFinder.Infrastructure/"]
COPY ["src/HomeFinder.Core/HomeFinder.Core.csproj", "HomeFinder.Core/"]


RUN dotnet restore "HomeFinder.Api/HomeFinder.Api.csproj"

# ソース全体をコピーしてビルド
COPY src/ .
RUN dotnet publish "HomeFinder.Api/HomeFinder.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
# JanSearch 外部商品検索 API の設定（楽天 Ichiba API）
ENV JanSearch__ExternalApi__BaseUrl="https://openapi.rakuten.co.jp/ichibaproduct/api/Product/Search/20250801"
ENV JanSearch__ExternalApi__ApiKey=""
ENV JanSearch__ExternalApi__ApplicationId=""
ENV JanSearch__ExternalApi__ApiKeyHeaderName="x-rapidapi-key"
ENV JanSearch__ExternalApi__ApiKeyQueryParameterName="accessKey"
ENV JanSearch__ExternalApi__HostHeaderName="x-rapidapi-host"
ENV JanSearch__ExternalApi__JanQueryParameter="productCode"
EXPOSE 8080
ENTRYPOINT ["dotnet", "HomeFinder.Api.dll"]