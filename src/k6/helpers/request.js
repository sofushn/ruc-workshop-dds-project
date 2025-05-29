import http from 'k6/http';
import { randomDecimal } from './checkUtils.js';

const testFile = open('../test.jpg', 'b')

export function performRequest(endpoint, tags) {
    if (endpoint.method === 'GET') {
        return http.get(endpoint.url, {tags});
    } else if (endpoint.method === 'POST') {
        let longitude = randomDecimal(-180, 180);
        let latitude = randomDecimal(-90, 90);

        const form = {
            latitude: latitude,
            longitude: longitude,
            mapId: endpoint.body.mapId,
            height: endpoint.body.height,
            file: http.file(testFile, 'test.jpg', 'image/jpeg')
        };
        return http.post(endpoint.url, form, {tags});    
    }
}