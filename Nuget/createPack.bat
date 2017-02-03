@echo off
set version=1.0.0
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)
set config="Release"
nuget pack ..\Src\MaintainableSelenium\MaintainableSelenium.MvcPages\MvcPages.csproj.nuspec -NoPackageAnalysis -verbosity detailed -o ./ -Version %version% -p Configuration="%config%"
nuget pack ..\Src\MaintainableSelenium\MaintainableSelenium.VisualAssertions\VisualAssertions.csproj.nuspec -NoPackageAnalysis -verbosity detailed -o ./ -Version %version% -p Configuration="%config%"