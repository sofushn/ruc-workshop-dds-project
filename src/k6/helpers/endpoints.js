import { isResponseImage } from './checkUtils.js';

const TEST_TYPE = __ENV.TEST_TYPE ?? 'GetImageById';
const IS_COMBINED_TEST = TEST_TYPE == 'Combined';
const BASE_URL = __ENV.BASE_URL ?? 'http://localhost:8080';

const endpoints = [
    {
        name: 'GetImageById',
        url: `${BASE_URL}/image-api/images/1.jpg`,
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
        url: `${BASE_URL}/api/map/1`,
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
        ratio: 0.01,
    },
    {
        name: 'GetWaypointsByMapId',
        url: `${BASE_URL}/api/map/1/waypoints`,
        method: 'GET',
        checks: {
            'status is 200': (r) => r.status === 200,
            'response time < 800ms': (r) => r.timings.duration < 800,
        },
        ratio: 0.05,
    },
    {
        name: 'PostWaypoint',
        url: `${BASE_URL}/api/waypoint`,
        method: 'POST',
        body: {
            mapId: 1,
            height: 50.0
        },
        checks: {
            'status is 201': (r) => r.status === 201,
            'response time < 1000ms': (r) => r.timings.duration < 1000,
        },
        ratio: 0.0001,
    },
];

export function getEndpoint(imageId) {
    endpoints[0].url = `${BASE_URL}/image-api/images/${imageId}.jpg`;

    return IS_COMBINED_TEST 
        ? pickEndpointWeighted() 
        : endpoints.find(e => e.name === TEST_TYPE) || endpoints[0];
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
