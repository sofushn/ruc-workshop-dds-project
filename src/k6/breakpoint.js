import http from 'k6/http';
import { check, sleep } from 'k6';
import { isResponseImage, randomDecimal } from './helpers/checkUtils.js';
import { tagWithCurrentStageProfile } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';
import { tagWithCurrentStageIndex } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';

const endpoints = [
    {
        name: 'GetImageById',
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
        name: 'GetMapById',
        url: 'http://localhost:8080/api/map/1',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
        ratio: 0.01,
    },
    {
        name: 'GetWaypointsByMapId',
        url: 'http://localhost:8080/api/map/1/waypoints',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
        ratio: 0.05,
    },
    {
        name: 'PostWaypoint',
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
        { duration: '30m', target: 10000 },
    ],
    thresholds: {
        'http_req_failed': [{ threshold: 'rate<0.01', abortOnFail: true, delayAbortVal: '20s' }],
        'checks': ['rate>0.95'], 
        
        'http_req_duration{endpoint:GetImageById}':[
            { threshold: 'p(99)<1000'},
            { threshold: 'p(95)<800', abortOnFail: true, delayAbortEval: '20s'},
            { threshold: 'max < 1800'}
        ],

        'http_req_duration{endpoint:GetMapById}':[
            { threshold: 'p(99)<1000'},
            { threshold: 'p(95)<800', abortOnFail: true, delayAbortEval: '20s'},
            { threshold: 'max < 1800'}
        ],

        'http_req_duration{endpoint:GetWaypointsByMapId}':[
            { threshold: 'p(99)<1000'},
            { threshold: 'p(95)<800', abortOnFail: true, delayAbortEval: '20s'},
            { threshold: 'max < 1800'}
        ],

        'http_req_duration{endpoint:PostWaypoint}':[
            { threshold: 'p(99)<1200'},
            { threshold: 'p(95)<1000', abortOnFail: true, delayAbortEval: '20s'},
            { threshold: 'max < 2000'},
        ]
    },
};

export default function () {
    tagWithCurrentStageIndex();
    tagWithCurrentStageProfile();

    const requestId = `${__VU}-${__ITER}`;
    let endpoint = combinedTest ? pickEndpointWeighted() : endpoints[urlIndex];

    const tags = {
        endpoint: endpoint.name,
        requestId: requestId,
        request_type: endpoint.method
    };

    let res;
    if (endpoint.method === 'GET') {
        res = http.get(endpoint.url, {tags});
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
        res = http.post(endpoint.url, form, {tags});    
    }
    check(res, endpoint.checks, tags);
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
