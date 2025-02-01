#!/bin/bash

export DOTNET_ENVIRONMENT=Development

cd /home/denis/development/NetCatHook_bin/
gnome-terminal --title="NetCatHook" -- bash -c "cd ./net8.0; ./NetCatHook.Scraper; sleep 7d"

#gnome-terminal --title="NetCatHook Tg Bot" -- bash -c "cd /home/denis/development/NetCatHook_bin/net8.0/; ./NetCatHook.Scraper; sleep 7d"
