dotnet sonarscanner begin /k:"Investment-App" /d:sonar.host.url="http://localhost:9000"  /d:sonar.token="sqp_3cdfc520445028d550a760d5fbdd1dc99fc3c92f"
dotnet build --no-incremental
dotcover.exe analyse .\coverConfig.xml
dotnet sonarscanner end /d:sonar.token="sqp_3cdfc520445028d550a760d5fbdd1dc99fc3c92f"
pause
