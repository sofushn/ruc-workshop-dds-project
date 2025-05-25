# Test types:
* stress-test.js. Purpuse is to test whether system can withhold extensive traffic
* spike-test.js. Purpose is to direct massive amount of traffic in short amount of time
* breakpoint-test.js. Purpose is to find out when system breaks

How to:

Run WINGET command:
* winget install k6 --source winget
* Run test script: k6 run NAME_OF_TEST.js

Save test result:
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"; if (!(Test-Path -Path "./results")) { New-Item -ItemType Directory -Path "../results" }; k6 run --summary-export="results/summary-$timestamp.json" --out csv="results/results-$timestamp.csv" ./breakpoint-test.js | Tee-Object -FilePath "results/output-$timestamp.txt"

Optional:
Remove tag "requestId" from tests, if performance is affected. If kept it will save current VUs ID and their iteration ID, for each request
