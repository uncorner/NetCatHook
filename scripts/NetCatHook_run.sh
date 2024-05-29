#!/bin/bash

export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=https://localhost:7171;http://localhost:5297

gnome-terminal --title="NetCatHook" --command="bash -c 'cd ./net8.0; ./NetCatHook.Scraper'"
