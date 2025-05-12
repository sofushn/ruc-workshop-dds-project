# Test types:
* smoke-test.js = Initial test. Testing if system works as intended, with very small load
* avg-load-test.js = 2 test. Purpose is to test average load. 
* stress-test.js = 3 test. Purpuse is to test whether system can withhold extensive traffic
* soak-test.js = 4 test. Purpose is to test how system performs over prolonged time of traffic load. Is reusing avg-load.js but with prolonged duration of 3 hours
* spike-test.js = 5 test. Purpose is to direct massive amount of traffic in short amount of time
* breakpoint-test.js = 6 test. Purpose is to find out when system breaks


Safe test result for breakpoint-test.js (imagestore)
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"; if (!(Test-Path -Path "./results")) { New-Item -ItemType Directory -Path "./results" }; k6 run --summary-export="results/summary-$timestamp.json" --out csv="results/results-$timestamp.csv" ./breakpoint-test.js | Tee-Object -FilePath "results/output-$timestamp.txt"
