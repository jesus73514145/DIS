# Usar la imagen oficial de .NET para la base
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

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "proyecto.csproj" -c Release -o /app/publish

# Instalar wkhtmltox y sus dependencias
FROM base AS final
RUN apt-get update \
    && apt-get install -y wget xfonts-75dpi xfonts-base libxrender1 libfontconfig1 libx11-xcb1 libxcb1 fontconfig libjpeg62-turbo libxext6 \
    && wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb \
    && dpkg -i wkhtmltox_0.12.6-1.buster_amd64.deb || true \
    && apt-get install -f -y \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* wkhtmltox_0.12.6-1.buster_amd64.deb

# Copiar los archivos publicados
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar el entrypoint
ENTRYPOINT ["dotnet", "proyecto.dll"]
