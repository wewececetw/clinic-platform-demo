# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 複製 csproj 先還原（利用 Docker layer cache）
COPY src/ClinicPlatform.Domain/ClinicPlatform.Domain.csproj src/ClinicPlatform.Domain/
COPY src/ClinicPlatform.Application/ClinicPlatform.Application.csproj src/ClinicPlatform.Application/
COPY src/ClinicPlatform.Infrastructure/ClinicPlatform.Infrastructure.csproj src/ClinicPlatform.Infrastructure/
COPY src/ClinicPlatform.WebAPI/ClinicPlatform.WebAPI.csproj src/ClinicPlatform.WebAPI/
RUN dotnet restore src/ClinicPlatform.WebAPI/ClinicPlatform.WebAPI.csproj

# 複製全部原始碼並發佈
COPY src/ src/
RUN dotnet publish src/ClinicPlatform.WebAPI/ClinicPlatform.WebAPI.csproj \
    -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5160
EXPOSE 5160

ENTRYPOINT ["dotnet", "ClinicPlatform.WebAPI.dll"]
