import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomDecimal } from "./helpers/checkUtils.js";
import { getEndpoint } from './helpers/endpoints.js';
import { getImageIds } from './helpers/images.js';
import { tagWithCurrentStageProfile } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';
import { tagWithCurrentStageIndex } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';

export const options = {
    stages: [
        { duration: "1m", target: 250 },
        { duration: "2m", target: 0 },
    ],
    thresholds: {
        'http_req_failed': [{ threshold: 'rate<0.01', abortOnFail: false, delayAbortVal: '20s' }],
        'checks': ['rate>0.95'], 
        
        'http_req_duration{endpoint:GetImageById}':[
            { threshold: 'p(99)<1000'},
            { threshold: 'p(95)<800', abortOnFail: false, delayAbortEval: '20s'},
            { threshold: 'max < 1800'}
        ],

        'http_req_duration{endpoint:GetMapById}':[
            { threshold: 'p(99)<1000'},
            { threshold: 'p(95)<800', abortOnFail: false, delayAbortEval: '20s'},
            { threshold: 'max < 1800'}
        ],

        'http_req_duration{endpoint:GetWaypointsByMapId}':[
            { threshold: 'p(99)<1000'},
            { threshold: 'p(95)<800', abortOnFail: false, delayAbortEval: '20s'},
            { threshold: 'max < 1800'}
        ],

        'http_req_duration{endpoint:PostWaypoint}':[
            { threshold: 'p(99)<1200'},
            { threshold: 'p(95)<1000', abortOnFail: false, delayAbortEval: '20s'},
            { threshold: 'max < 2000'},
        ]
    },
};
export function setup() {
    const imageIds = getImageIds();
    console.log(`Fetched ${imageIds.length} image IDs.`);
    console.log(`Image Id: ${imageIds[0]}`);
    return { 
        imageIds,
    };
}

export default function (data) {
    tagWithCurrentStageIndex();
    tagWithCurrentStageProfile();
    
    const requestId = `${__VU}-${__ITER}`;
    const imageId = data.imageIds[Math.floor(Math.random() * data.imageIds.length)];
    const endpoint = getEndpoint(imageId);

    const tags = {
        endpoint: endpoint.name,
        requestId: requestId,
        request_type: endpoint.method
    };

    let res;
    if (endpoint.method === 'GET') {
        res = http.get(endpoint.url, { tags });
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
        res = http.post(endpoint.url, form, { tags });    
    }
    check(res, endpoint.checks, tags);
    sleep(1);
}
