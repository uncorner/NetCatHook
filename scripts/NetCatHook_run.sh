#!/bin/bash

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=https://localhost:7171;http://localhost:5297

cd /home/denis/development/NetCatHook_bin/
gnome-terminal --title="NetCatHook" -- bash -c "cd ./net8.0; ./NetCatHook.Scraper; sleep 7d"

#gnome-terminal --title="NetCatHook Tg Bot" -- bash -c "cd /home/denis/development/NetCatHook_bin/net8.0/; ./NetCatHook.Scraper; sleep 7d"
