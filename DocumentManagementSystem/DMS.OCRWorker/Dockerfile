FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DMS.OCRWorker/DMS.OCRWorker.csproj", "DMS.OCRWorker/"]
COPY ["DMS.DAL/DMS.DAL.csproj", "DMS.DAL/"]
COPY ["DMS.Domain/DMS.Domain.csproj", "DMS.Domain/"]
RUN dotnet restore "DMS.OCRWorker/DMS.OCRWorker.csproj"
COPY . .
WORKDIR "/src/DMS.OCRWorker"
RUN dotnet build "DMS.OCRWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.OCRWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS.OCRWorker.dll"]