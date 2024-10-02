# Etapa 1: Usar la imagen oficial de .NET para la base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Etapa 2: Usar la imagen SDK para construir la app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["proyecto.csproj", "./"]
RUN dotnet restore "./proyecto.csproj"
COPY . .
RUN dotnet build "proyecto.csproj" -c Release -o /app/build

# Etapa 3: Publicar la app
FROM build AS publish
RUN dotnet publish "proyecto.csproj" -c Release -o /app/publish

# Etapa 4: Instalar wkhtmltox y dependencias sin libssl1.1
FROM base AS final
WORKDIR /app

# Actualizar paquetes e instalar dependencias necesarias
RUN apt-get update && apt-get install -y --no-install-recommends \
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

# Descargar e instalar una versión de wkhtmltox que no dependa de libssl1.1
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.5-1/wkhtmltox_0.12.5-1.bionic_amd64.deb && \
    dpkg -i wkhtmltox_0.12.5-1.bionic_amd64.deb || apt-get install -f -y

# Copiar la aplicación publicada
COPY --from=publish /app/publish .

# Configurar el punto de entrada
ENTRYPOINT ["dotnet", "proyecto.dll"]