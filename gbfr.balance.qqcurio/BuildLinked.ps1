# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/gbfr.balance.qqcurio/*" -Force -Recurse
dotnet publish "./gbfr.balance.qqcurio.csproj" -c Release -o "$env:RELOADEDIIMODS/gbfr.balance.qqcurio" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location