FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/assistant.Ai/Application/Application.csproj ./assistant.Ai/Application/
COPY src/assistant.Ai/Api/Api.csproj ./assistant.Ai/Api/


RUN dotnet restore ./assistant.Ai/Api/Api.csproj

COPY src/assistant.Ai/Application ./assistant.Ai/Application
COPY src/assistant.Ai/Api ./assistant.Ai/Api

RUN dotnet publish ./assistant.Ai/Api/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

COPY src/assistant.Ai/Api/Prompts ./Prompts

ENTRYPOINT ["dotnet", "Api.dll"]