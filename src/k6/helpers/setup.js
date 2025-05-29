import http from 'k6/http';

const testFile = http.file(open('../test.jpg', 'b'), 'test.jpg')

function getImageIds () {
    const BASE_URL = __ENV.BASE_URL ?? 'http://localhost:8080';
    const response = http.get(`${BASE_URL}/api/map/1/waypoints`);
    
    if (response.status !== 200) {
        throw new Error(`Failed to fetch image IDs: ${response.status}`);
    }
    
    const imageIds = response.json().map(w => w.imageId);
    
    if (imageIds.length === 0) {
        throw new Error('No image IDs found');
    }
    
    return imageIds;
}

export function defaultSetup() {
    const testType = __ENV.TEST_TYPE ?? 'GetImageById';
    let imageIds = [];
    if (testType == 'GetImageById' || testType == 'Combined') {
        imageIds = getImageIds();
        console.log(`Fetched ${imageIds.length} image IDs.`);
        console.log(`Image Id: ${imageIds[0]}`);
    }
    
    return { 
        imageIds,
        testFile,
    };
}
