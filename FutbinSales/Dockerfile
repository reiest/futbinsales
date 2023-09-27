FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FutbinSales/FutbinSales.csproj", "FutbinSales/"]
RUN dotnet restore "FutbinSales/FutbinSales.csproj"
COPY . .
WORKDIR "/src/FutbinSales"
RUN dotnet build "FutbinSales.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FutbinSales.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FutbinSales.dll"]
