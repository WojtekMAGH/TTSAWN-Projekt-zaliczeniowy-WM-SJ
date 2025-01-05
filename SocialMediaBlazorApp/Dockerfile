FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SocialMediaBlazorApp/SocialMediaBlazorApp.csproj", "SocialMediaBlazorApp/"]
RUN dotnet restore "SocialMediaBlazorApp/SocialMediaBlazorApp.csproj"

COPY . .
WORKDIR "/src/SocialMediaBlazorApp"
RUN dotnet build "SocialMediaBlazorApp.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SocialMediaBlazorApp.csproj" -c Release -o /app/publish

# Create the runtime image (web server to host the app - needed as we have Blazor WebAssemly that uses static files)
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY SocialMediaBlazorApp/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80