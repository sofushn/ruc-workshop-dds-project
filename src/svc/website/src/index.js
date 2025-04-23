let imageHostUrl = "http://localhost:5008/images/";
let mapImage = document.getElementById('mapImage');
mapImage.src = imageHostUrl+"2dmap.jpg";

let locationData = [{
    "latitude": 55.651459,
    "longitude": 12.134499,
    "image_id": "1"
}, {
    "latitude": 55.653372,
    "longitude": 12.140358,
    "image_id": "2"
}, {
    "latitude": 55.652658,
    "longitude": 12.139993,
    "image_id": "3"
}, {
    "latitude": 55.654111,
    "longitude": 12.138877,
    "image_id": "4"
}];

let mapBoundaries = {
    northEast: { lat: 55.655294, lng: 12.144353 },
    southWest: { lat: 55.651107, lng: 12.134139 },
};


let panorama = pannellum.viewer('panorama', {
    "type": "equirectangular",
    "panorama": imageHostUrl+locationData[0].image_id + ".jpg",
    "autoLoad": true,
    "showControls": false,
});


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
        panorama.destroy();
        panorama = pannellum.viewer('panorama', {
            "type": "equirectangular",
            "panorama": imageHostUrl+location.image_id + ".jpg",
            "autoLoad": true,
            "showControls": false,
        });
    });

    mapImage.parentElement.appendChild(button);
});