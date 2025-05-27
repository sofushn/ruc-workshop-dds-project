import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomDecimal } from "./helpers/checkUtils.js";
import { getEndpoint } from './helpers/endpoints.js';
import { getImageIds } from './helpers/images.js';
import { tagWithCurrentStageProfile } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';
import { tagWithCurrentStageIndex } from 'https://jslib.k6.io/k6-utils/1.3.0/index.js';

export const options = {
    stages: [
        { duration: "2m", target: 50 },
        { duration: "5m", target: 50 },
        { duration: "2m", target: 0 },
    ],
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
            File: http.file(open(endpoint.body.filePath, 'b'), 'test.jpg'),
        };
        res = http.post(endpoint.url, form);    
    }
    check(res, endpoint.checks);
    sleep(1);
}
