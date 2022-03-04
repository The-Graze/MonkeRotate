# Needs to be at least that version, or mmm can't read the archive
#Requires -Modules @{ ModuleName="Microsoft.PowerShell.Archive"; ModuleVersion="1.2.3" }
$Name = "MonkeSwim" # Replace with your mods name
$Version = "v1.0.6"

mkdir BepInEx\plugins\$Name
mkdir BepInEx\plugins\MonkeMapLoader\CustomMaps

cp Swim\bin\Release\netstandard2.0\$Name.dll BepInEx\plugins\$Name\
cp GravityTechnicalTest.gtmap BepInEx\Plugins\MonkeMapLoader\CustomMaps\

Compress-Archive -force .\BepInEx\ $Name-$Version.zip

rmdir .\BepInEx\ -Recurse