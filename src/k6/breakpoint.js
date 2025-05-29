import { check, sleep } from 'k6';
import { defaultSetup } from './helpers/setup.js';
import { performRequest } from './helpers/request.js';
import { getEndpoint } from './helpers/endpoints.js';
import { tagWithCurrentStageProfile } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';
import { tagWithCurrentStageIndex } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';

export const options = {
    stages: [
        { duration: '10m', target: 500 },
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

export function setup() {
    return defaultSetup();
}

export default function (data) {
    tagWithCurrentStageIndex();
    tagWithCurrentStageProfile();

    const imageId = data.imageIds[Math.floor(Math.random() * data.imageIds.length)];
    const endpoint = getEndpoint(imageId);

    const tags = {
        endpoint: endpoint.name,
        requestId: `${__VU}-${__ITER}`,
        request_type: endpoint.method
    };

    let res = performRequest(endpoint, data.testFile, tags);
    check(res, endpoint.checks, tags);
    sleep(1);
}
