import http from 'k6/http';
import { check, sleep } from 'k6';
import { open } from 'k6/experimental/fs';
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
        url: 'http://localhost:8080/api/map/1/waypoints',
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
    },
    {
        url: 'http://localhost:8080/api/waypoints/1',
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
    },
];

const urlIndex = __ENV.URL_INDEX ? parseInt(__ENV.URL_INDEX) : 0;
const endpoint = endpoints[urlIndex];

export const options = {
    stages: [
        { duration: "2m", target: 1000 },
        { duration: "1m", target: 0 },
    ],
};

export default function () {
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