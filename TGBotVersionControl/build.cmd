@echo OFF
cd HaltMalKurz
git pull
nuget restore
devenv /Build Release "HaltMalKurz.sln"