let panorama = pannellum.viewer('panorama', {
    "type": "equirectangular",
    "panorama": "data/1.jpg",
    "autoLoad": true,
    "showControls": false,
});

let locationData = [{
    "latitude": 55.651459,
    "longitude": 12.134499,
    "image": "data/1.jpg"
}, {
    "latitude": 55.653372,
    "longitude": 12.140358,
    "image": "data/2.jpg"
}, {
    "latitude": 55.652658,
    "longitude": 12.139993,
    "image": "data/3.jpg"
}, {
    "latitude": 55.654111,
    "longitude": 12.138877,
    "image": "data/4.jpg"
}];

let mapBoundaries = {
    northEast: { lat: 55.655294, lng: 12.144353 },
    southWest: { lat: 55.651107, lng: 12.134139 },
};

let mapImage = document.getElementById('mapImage');
locationData.forEach((location, index) => {
    let button = document.createElement('button');
    button.innerHTML = index + 1; // Button label
    button.style.position = 'absolute';
    button.style.width = '10px';
    button.style.height = '10px';
    button.style.backgroundColor = 'red';
    button.style.borderRadius = '50%';
    button.style.left = ((location.longitude - mapBoundaries.southWest.lng) / 
                         (mapBoundaries.northEast.lng - mapBoundaries.southWest.lng) * 100) + '%';
    button.style.top = ((mapBoundaries.northEast.lat - location.latitude) / 
                        (mapBoundaries.northEast.lat - mapBoundaries.southWest.lat) * 100) + '%';
    button.style.transform = 'translate(-50%, -50%)';
    button.style.zIndex = 1000;

    button.addEventListener('click', () => {
        panorama = pannellum.viewer('panorama', {
            "type": "equirectangular",
            "panorama": location.image,
            "autoLoad": true,
            "showControls": false,
        });
    });

    mapImage.parentElement.appendChild(button);
});