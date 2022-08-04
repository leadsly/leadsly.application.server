FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base 
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

WORKDIR /src
COPY "./Leadsly.Application.Server/Leadsly.Application.Server.sln" ./Leadsly.Application.Server/
COPY "../Leadsly.Application.Model/*.csproj" ./Leadsly.Application.Model/
COPY "./Leadsly.Application.Server/Leadsly.Application.Api/*.csproj" ./Leadsly.Application.Server/Leadsly.Application.Api/
COPY "./Leadsly.Application.Server/Leadsly.Domain/*.csproj" ./Leadsly.Application.Server/Leadsly.Domain/
COPY "./Leadsly.Application.Server/Leadsly.Infrastructure/*.csproj" ./Leadsly.Application.Server/Leadsly.Infrastructure/

WORKDIR /src/Leadsly.Application.Server
RUN dotnet restore

WORKDIR /src
COPY . .

WORKDIR "/src/Leadsly.Application.Model"
RUN dotnet build -c Release -o /../../app

WORKDIR "/src/Leadsly.Application.Server/Leadsly.Domain"
RUN dotnet build -c Release -o /../../app

WORKDIR "/src/Leadsly.Application.Server/Leadsly.Application.Api"
RUN dotnet build -c Release -o /../../app
RUN rm -rf /src/Leadsly.Application.Server/Leadsly.Application.Api/ClientApp

WORKDIR "/src/Leadsly.Application.Server/Leadsly.Infrastructure"
RUN dotnet build -c Release -o /../../app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENV AWS_ACCESS_KEY_ID="AKIA2KIVUGORHZXNMOVT"
ENV AWS_REGION="us-east-1"
ENV AWS_SECRET_ACCESS_KEY="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"

WORKDIR /app

ENTRYPOINT ["dotnet", "Leadsly.Application.Api.dll"]