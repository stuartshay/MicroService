dotnet sonarscanner begin /k:"MicroService.Api" /d:sonar.host.url="http://192.168.1.172:9100" /d:sonar.exclusions=Program.cs,**/Extensions/**/*.cs  /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml /d:sonar.login="***REMOVED-SONAR-TOKEN***"
dotnet build --no-incremental
dotnet-coverage collect 'dotnet test' -f xml  -o 'coverage.xml'
dotnet sonarscanner end /d:sonar.login="***REMOVED-SONAR-TOKEN***"