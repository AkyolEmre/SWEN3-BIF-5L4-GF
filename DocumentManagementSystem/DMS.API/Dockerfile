FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DMS.API/DMS.API.csproj", "DMS.API/"]
COPY ["DMS.DAL/DMS.DAL.csproj", "DMS.DAL/"]
COPY ["DMS.Domain/DMS.Domain.csproj", "DMS.Domain/"]
RUN dotnet restore "DMS.API/DMS.API.csproj"
COPY . .
WORKDIR "/src/DMS.API"
RUN dotnet build "DMS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS.API.dll"]