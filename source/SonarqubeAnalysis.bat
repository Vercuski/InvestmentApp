dotnet sonarscanner begin /k:"InvestmentApp" /d:sonar.host.url="http://localhost:9000" /d:sonar.token="sqp_d120164d75fab5e93bd07f63fa5b5f266f482392" /d:sonar.cs.dotcover.reportsPaths=dotCover.Output.html
dotnet build --no-incremental
dotcover.exe analyse .\coverConfig.xml
dotnet sonarscanner end /d:sonar.token="sqp_d120164d75fab5e93bd07f63fa5b5f266f482392"
pause
