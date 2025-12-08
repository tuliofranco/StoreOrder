
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/order.Core/order.Core.csproj ./Order.Core/     
COPY src/order.Infrastructure/order.Infrastructure.csproj ./Order.Infrastructure/    
COPY src/order.Api/order.Api.csproj ./order.Api/

RUN dotnet restore ./order.Api/order.Api.csproj

COPY src/order.Core ./order.Core
COPY src/order.Infrastructure ./order.Infrastructure
COPY src/order.Api ./order.Api

RUN dotnet publish ./order.Api/order.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "order.Api.dll"]