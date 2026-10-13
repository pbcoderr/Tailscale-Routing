param([string]$OutputDirectory = (Join-Path $PSScriptRoot '..\Windows'))
$ErrorActionPreference='Stop'
$compiler=Join-Path $env:SystemRoot 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$sources=Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object FullName
& $compiler /nologo /target:winexe /platform:x64 /optimize+ "/out:$OutputDirectory\Tailscale Routing.exe" "/win32manifest:$PSScriptRoot\app.manifest" "/win32icon:$PSScriptRoot\Assets\TailscaleRouting.ico" /r:Microsoft.VisualBasic.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:System.Web.Extensions.dll /r:System.IO.Compression.dll "/resource:$PSScriptRoot\Assets\TailscaleRouting.ico,TailscaleRouting.App.ico" "/resource:$PSScriptRoot\Engine\sing-box-1.14.2-windows-amd64.zip,TailscaleRouting.SingBox.zip" $sources
if($LASTEXITCODE -ne 0) { throw 'Build failed' }
Copy-Item -LiteralPath "$PSScriptRoot\App.config" -Destination "$OutputDirectory\Tailscale Routing.exe.config"
New-Item -ItemType Directory -Force -Path "$OutputDirectory\Engine" | Out-Null
foreach($name in @('LICENSE','COPYING','SOURCE.md','SOURCE.zip')) { Copy-Item -LiteralPath "$PSScriptRoot\Engine\$name" -Destination "$OutputDirectory\Engine\$name" }
Copy-Item -LiteralPath "$PSScriptRoot\COPYRIGHT" -Destination $OutputDirectory
Copy-Item -LiteralPath "$PSScriptRoot\Examples" -Destination $OutputDirectory -Recurse -Force
Copy-Item -LiteralPath "$PSScriptRoot\README.md" -Destination $OutputDirectory
Write-Output "Built: $OutputDirectory\Tailscale Routing.exe"
