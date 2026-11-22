FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/MoedaEstudantil/MoedaEstudantil.csproj src/MoedaEstudantil/
RUN dotnet restore src/MoedaEstudantil/MoedaEstudantil.csproj
COPY src/MoedaEstudantil/ src/MoedaEstudantil/
RUN dotnet publish src/MoedaEstudantil/MoedaEstudantil.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
RUN mkdir -p /app/dados /var/dados \
    && chown -R "$APP_UID:$APP_UID" /app /var/dados
USER $APP_UID
ENTRYPOINT ["dotnet", "MoedaEstudantil.dll"]
