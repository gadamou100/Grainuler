#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GranulerSampleClient/GranulerSampleClient.csproj", "GranulerSampleClient/"]
COPY ["Grainuler/Grainuler.csproj", "Grainuler/"]
COPY ["Grainuler.Abstractions/Grainuler.Abstractions.csproj", "Grainuler.Abstractions/"]
COPY ["Grainuler.DataTransferObjects/Grainuler.DataTransferObjects.csproj", "Grainuler.DataTransferObjects/"]
RUN dotnet restore "GranulerSampleClient/GranulerSampleClient.csproj"
COPY . .
WORKDIR "/src/GranulerSampleClient"
RUN dotnet build "GranulerSampleClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GranulerSampleClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GranulerSampleClient.dll"]