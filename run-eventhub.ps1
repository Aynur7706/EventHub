$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

dotnet run --project .\EventHub.Web\EventHub.Web.csproj --no-launch-profile --urls http://localhost:5088
