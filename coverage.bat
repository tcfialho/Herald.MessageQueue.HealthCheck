dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet test /maxcpucount:1 /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:CoverletOutput="../../coverage/coverage.xml" /p:Include="[Herald.MessageQueue.HealthCheck*.*]*"
reportgenerator "-reports:../coverage/coverage.xml" "-targetdir:../coverage"
start "" "..\coverage\index.htm"