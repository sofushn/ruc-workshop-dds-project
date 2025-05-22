import http from 'k6/http';
import { check, sleep } from 'k6';
import { isResponseImage, randomDecimal } from "./helpers/checkUtils.js";


const endpoints = [
    {
        url: 'http://localhost:8080/image-api/images/1.jpg',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
            'response is an image': (r) => isResponseImage(r),
        },
        ratio: 1,
    },
    {
        url: 'http://localhost:8080/api/map/1',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
        ratio: 0.01,
    },
    {
        url: 'http://localhost:8080/api/map/1/waypoints',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
        ratio: 0.05,
    },
    {
        url: 'http://localhost:8080/api/waypoint/1',
        method: 'POST',
        body: {
            mapId: 1,
            height: 50.0,
            filePath: 'test.jpg'
        },
        checks: {
            'status is 201': (r) => r.status === 201,
            'response time < 1000ms': (r) => r.timings.duration < 1000,
        },
        ratio: 0.0001,
    },
];

const urlIndex = __ENV.URL_INDEX ? parseInt(__ENV.URL_INDEX) : 0;
const combinedTest = __ENV.COMBINED_TEST ? __ENV.COMBINED_TEST === 'true' : false;

export const options = {
    stages: [
        { duration: "30m", target: 10000 },
    ],
    thresholds: {
        "http_req_duration": [
            { threshold: "p(95)<800", abortOnFail: true, delayAbortEval: "20s" },
            { threshold: "avg < 500", abortOnFail: true, delayAbortVal: "20s" }
        ], 
        "http_req_failed": [{ threshold: "rate<0.01", abortOnFail: true, delayAbortVal: "20s" }],
        "checks": ["rate>0.95"], 
    },
};

export default function () {
    let endpoint = combinedTest ? pickEndpointWeighted() : endpoints[urlIndex];
    let res;
    if (endpoint.method === 'GET') {
        res = http.get(endpoint.url);
    } else if (endpoint.method === 'POST') {
        let longitude = randomDecimal(-180, 180);
        let latitude = randomDecimal(-90, 90);

        const form = {
            Latitude: latitude,
            Longitude: longitude,
            MapId: String(endpoint.body.mapId),
            Height: String(endpoint.body.height),
            File: http.file(open(endpoint.body.filePath, 'b'), 'test.jpg'),
        };
        res = http.post(endpoint.url, form);    
    }
    check(res, endpoint.checks);
    sleep(1);
}

function pickEndpointWeighted() {
    const total = endpoints.reduce((sum, e) => sum + e.ratio, 0);
    let r = Math.random() * total;
    let cumulative = 0;
    for (const e of endpoints) {
        cumulative += e.ratio;
        if (r < cumulative) return e;
    }
    return endpoints[0];
}
