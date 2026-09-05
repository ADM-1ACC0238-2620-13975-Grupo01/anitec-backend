FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Anitec.Platform/Anitec.Platform.csproj Anitec.Platform/
RUN dotnet restore Anitec.Platform/Anitec.Platform.csproj

COPY . .
RUN dotnet publish Anitec.Platform/Anitec.Platform.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

ENTRYPOINT ["dotnet", "Anitec.Platform.dll"]
