//k6 run k6tests.js --env URL_INDEX=1 --env TEST_TYPE=spike

import http from 'k6/http';
import { check, sleep } from 'k6';
import {isResponseImage } from "../helpers/checkUtils.js";


const endpoints = [
    {
        url: 'http://localhost:8080/image-api/images/1.jpg',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
            'response is an image': (r) => isResponseImage(r),
        },
    },
    {
        url: 'http://localhost:8080/api/map/1',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
    },
    {
        url: 'http://localhost:8080/api/map/1/coordinates',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
    },
    {
        url: 'http://localhost:8080/api/coordinates/1',
        method: 'POST',
        body: {
            file: http.file(open('Trollface.jpg', 'b'), 'Trollface.jpg'),
        },
        checks: {
            'status is 201': (r) => r.status === 201,
            'response time < 1000ms': (r) => r.timings.duration < 1000,
        },
    },
];

const urlIndex = __ENV.URL_INDEX ? parseInt(__ENV.URL_INDEX) : 0;
const endpoint = endpoints[urlIndex];
const testType = __ENV.TEST_TYPE || 'breakpoint';

let options = {};

if (testType === 'breakpoint') {
    options = {
        stages: [
            { duration: "30m", target: 10000 },
        ],
    };
} else if (testType === 'spike') {
    options = {
        stages: [
            { duration: "2m", target: 1000 }, 
            { duration: "1m", target: 0 },
        ],
    };
} else if (testType === 'stress') {
    options = {
        stages: [
            { duration: "2m", target: 500 }, 
            { duration: "5m", target: 500 }, 
            { duration: "2m", target: 0 },
        ],
    };
} else {
    throw new Error(`Unknown TEST_TYPE: ${testType}`);
}

export { options };

export default function () {
    let res;
    if (endpoint.method === 'GET') {
        res = http.get(endpoint.url);
    } else if (endpoint.method === 'POST') {
        res = http.post(endpoint.url, endpoint.body || {});
    }

    check(res, endpoint.checks);
    sleep(1);
}