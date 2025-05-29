import http from 'k6/http';
import { randomDecimal } from './checkUtils.js';

export function performRequest(endpoint, testFile, tags) {
    if (endpoint.method === 'GET') {
        return http.get(endpoint.url, {tags});
    } else if (endpoint.method === 'POST') {
        let longitude = randomDecimal(-180, 180);
        let latitude = randomDecimal(-90, 90);

        const form = {
            Latitude: latitude,
            Longitude: longitude,
            MapId: String(endpoint.body.mapId),
            Height: String(endpoint.body.height),
            File: testFile,
        };
        return http.post(endpoint.url, form, {tags});    
    }
}