# -------------------------------------------------
# 1. Build-Stage (nur .NET SDK)
# -------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiere nur csproj-Dateien → schneller Restore
COPY ["DMS.OCRWorker/DMS.OCRWorker.csproj", "DMS.OCRWorker/"]
COPY ["DMS.Domain/DMS.Domain.csproj", "DMS.Domain/"]
# (DMS.DAL brauchst du hier NICHT, wenn du nur Domain referenzierst)

RUN dotnet restore "DMS.OCRWorker/DMS.OCRWorker.csproj"

# Kopiere Rest des Codes
COPY . .

WORKDIR "/src/DMS.OCRWorker"
RUN dotnet publish "DMS.OCRWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------------------------------
# 2. Final-Stage (Runtime + Tesseract + Pdfium)
# -------------------------------------------------
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final

# System-Abhängigkeiten für Tesseract & PdfiumViewer
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    libtesseract-dev \
    libleptonica-dev \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# tessdata-Ordner (Sprachdateien) kopieren – MUSS im Repo liegen!
COPY DMS.OCRWorker/tessdata ./tessdata

# Optional: Pdfium-Binaries (wenn du PdfiumViewer verwendest)
# Falls du PdfiumViewer.Native.x86_64.v8-xfa nutzt, kopiere die DLLs:
# COPY DMS.OCRWorker/runtimes/linux-x64/native/* ./

ENTRYPOINT ["dotnet", "DMS.OCRWorker.dll"]