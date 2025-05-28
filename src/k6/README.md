# Test types:
* stress-test.js. Purpuse is to test whether system can withhold extensive traffic
* spike-test.js. Purpose is to direct massive amount of traffic in short amount of time
* breakpoint-test.js. Purpose is to find out when system breaks

How to:

- Install k6 locally or run through docker (see guide below)
  - `winget install k6 --source winget`
- Run test script: `k6 run NAME_OF_TEST.js`

Tests:
- breakpoint
- spike
- stress


Save test result:
```
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"; if (!(Test-Path -Path "./results")) { New-Item -ItemType Directory -Path "../results" }; k6 run --summary-export="results/summary-$timestamp.json" --out csv="results/results-$timestamp.csv" ./breakpoint-test.js | Tee-Object -FilePath "results/output-$timestamp.txt"
```

Optional:
Remove tag "requestId" from tests, if performance is affected. If kept it will save current VUs ID and their iteration ID, for each request

## Run in docker

```
cd ./src/k6
docker run --rm -it --entrypoint /bin/sh -v ${PWD}:/scripts grafana/k6
```

Navigate to `/scripts` folder and run k6 command.

```
timestamp=$(date +"%Y%m%d_%H%M%S")
mkdir results
timestamp=$(date +"%Y%m%d_%H%M%S") k6 run <test> --summary-export=results/summary-${timestamp}.json --out csv=results/results-${timestamp}.csv --out json=results/results-${timestamp}.json

timestamp=$(date +"%Y%m%d_%H%M%S") k6 run stress.js --summary-export=results/summary-${timestamp}.json --out csv=results/results-${timestamp}.csv --out json=results/results-${timestamp}.json --env BASE_URL=http://192.168.0.2:8080
```

### Tests


__breakpoint__:
```
api: stages: [
        { duration: '10m', target: 10000 },
    ]
image: stages: [
        { duration: '10m', target: 500 },
    ]
```

__spike__:
```
api: stages: [
        { duration: "30s", target: 1000 },
        { duration: "1m", target: 0 },
    ]
        ,
image: stages: [
        { duration: "15s", target: 100 },
        { duration: "1m", target: 0 },
    ]
```

__stress__:
```
api: stages: [
        { duration: "1m", target: 500 },
        { duration: "4m", target: 500 },
        { duration: "1m", target: 0 },
    ],
image: stages: [
        { duration: '1m', target: 50 },
        { duration: '4m', target: 50 },
        { duration: '1m', target: 0 },
    ],
```


## Options

| Env name | Possible values | Default
| --- | --- | --- |
| `TEST_TYPE` | `GetImageById`, `GetMapById`, `GetWaypointsByMapId`, `PostWaypoint`, `Combined` | `GetImageById` |
| `BASE_URL` | Any url (including http/https in the beginning and possible port number) | `http://localhost:8080` |
