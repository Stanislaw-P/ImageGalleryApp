# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Временно убираем USER отсюда, перенесем в самый конец
EXPOSE 8080
EXPOSE 8081

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ImageGalleryApp/ImageGalleryApp.csproj", "ImageGalleryApp/"]
COPY ["ImageGallery.Db/ImageGallery.Db.csproj", "ImageGallery.Db/"]
RUN dotnet restore "./ImageGalleryApp/ImageGalleryApp.csproj"
COPY . .
WORKDIR "/src/ImageGalleryApp"
RUN dotnet build "./ImageGalleryApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImageGalleryApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# СОЗДАЕМ ПАПКУ И УСТАНАВЛИВАЕМ ПРАВА ДО СМЕНЫ ПОЛЬЗОВАТЕЛЯ
RUN mkdir -p /app/wwwroot/uploads && \
    chmod 777 /app/wwwroot/uploads

# Теперь переключаемся на непривилегированного пользователя
USER $APP_UID

ENTRYPOINT ["dotnet", "ImageGalleryApp.dll"]