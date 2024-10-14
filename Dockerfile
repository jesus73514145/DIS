# Usar la imagen oficial de .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Usar la imagen SDK para construir la app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["proyecto.csproj", "./"]
RUN dotnet restore "./proyecto.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "proyecto.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "proyecto.csproj" -c Release -o /app/publish

# Instalar wkhtmltox y sus dependencias
FROM base AS final
RUN apt-get update && apt-get install -y \
    wget \
    xfonts-75dpi \
    xfonts-base \
    libxrender1 \
    libfontconfig1 \
    libx11-xcb1 \
    libxcb1 \
    fontconfig \
    libjpeg62-turbo \
    libxext6

# Descargar y instalar wkhtmltox
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb && \
    apt install -y ./wkhtmltox_0.12.6-1.buster_amd64.deb && \
    rm wkhtmltox_0.12.6-1.buster_amd64.deb

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "proyecto.dll"]
