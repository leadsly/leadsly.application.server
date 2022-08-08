FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base 
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY "./Leadsly.Application.Server.sln" .
COPY "./Leadsly.Application.Model/*.csproj" ./Leadsly.Application.Model/
COPY "./Leadsly.Application.Api/*.csproj" ./Leadsly.Application.Api/
COPY "./Leadsly.Domain/*.csproj" ./Leadsly.Domain/
COPY "./Leadsly.Infrastructure/*.csproj" ./Leadsly.Infrastructure/

RUN dotnet restore
COPY . .

WORKDIR "/src//Leadsly.Application.Model"
RUN dotnet build -c Release -o /app

WORKDIR "/src/Leadsly.Domain"
RUN dotnet build -c Release -o /app

WORKDIR "/src/Leadsly.Infrastructure"
RUN dotnet build -c Release -o /app

WORKDIR "/src/Leadsly.Application.Api"
RUN dotnet build -c Release -o /app

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