# Etapa 1: Construcción de la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo .csproj y restaurar las dependencias
COPY ["ProjectManager.Api.User/ProjectManager.Api.User.csproj", "ProjectManager.Api.User/"]
RUN dotnet restore "ProjectManager.Api.User/ProjectManager.Api.User.csproj"

# Copiar todo el código fuente y compilar la aplicación
COPY . .
WORKDIR "/src/ProjectManager.Api.User"
RUN dotnet build "ProjectManager.Api.User.csproj" -c Release -o /app/build

# Publicar la aplicación en el directorio /app/publish
FROM build AS publish
RUN dotnet publish "ProjectManager.Api.User.csproj" -c Release -o /app/publish

# Etapa 2: Crear la imagen final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar los archivos publicados al contenedor final
COPY --from=publish /app/publish .

# Configurar las variables de entorno para Kestrel (puertos específicos)
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Exponer los puertos que se utilizarán
EXPOSE 80
EXPOSE 443

# Comando de entrada para ejecutar la aplicación
ENTRYPOINT ["dotnet", "ProjectManager.Api.User.dll"]
