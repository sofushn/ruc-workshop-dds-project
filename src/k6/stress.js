import { check, sleep } from 'k6';
import { defaultSetup } from './helpers/setup.js';
import { performRequest } from './helpers/request.js';
import { getEndpoint } from './helpers/endpoints.js';
import { tagWithCurrentStageProfile } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';
import { tagWithCurrentStageIndex } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';

export const options = {
    stages: [
        { duration: "1m", target: 50 },
        { duration: "4m", target: 50 },
        { duration: "1m", target: 0 },
    ],
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
