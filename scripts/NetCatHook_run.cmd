set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=https://localhost:7171;http://localhost:5297

start /B "NetCatHook" /D "net7.0" "NetCatHook.Scraper.exe"
pause
