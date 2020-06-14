
![Build](https://github.com/gobixm/insync/workflows/Build/badge.svg?branch=master)

# Description

Service and cli to replicate one folder to another.

# Installation
```bash
# service
dotnet tool install --global Gobi.InSync.Service

# cli
dotnet tool install --global Gobi.InSync.Cli
```

# Autostart
For windows run powershell script
```ps
New-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name "insyncd" -Value "c:\Users\'user name'\.dotnet\tools\insyncd.exe"  -PropertyType "String"
```
For linux create systemd unit

# Usage
```bash
# view help
insync

# add watch
insync add -s <absolute_source_folder> -t <absolute_target_folder>

# remove watch
insync remove -s <absolute_source_folder> -t <absolute_target_folder>

# or, to remove all watches on source
insync remove -s <absolute_source_folder>

# list current watches
insync list
```