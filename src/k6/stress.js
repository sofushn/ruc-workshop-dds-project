import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomDecimal } from "./helpers/checkUtils.js";
import { getEndpoint } from './helpers/endpoints.js';
import { defaultSetup } from './helpers/setup.js';
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
            File: data.testFile,
        };
        res = http.post(endpoint.url, form);    
    }
    check(res, endpoint.checks);
    sleep(1);
}
