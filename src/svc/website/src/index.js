let imageHostUrl = "http://localhost:8080/image-api/images/";
let apiHostUrl = "http://localhost:8080/api/";
let mapImage = document.getElementById('mapImage');


let panorama = pannellum.viewer('panorama', {
    "type": "equirectangular",
    "autoLoad": true,
    "showControls": false,
});

function createMap(data) {
mapImage.src = imageHostUrl + data.imageId + ".jpg";
mapBoundaries = {
    northEast: { lat: data.neLatitude, lng: data.neLongitude },
    southWest: { lat: data.swLatitude, lng: data.swLongitude },
};
}

function updateMap(data) {
    data.forEach((location, index) => {
        if (location.longitude < mapBoundaries.southWest.lng || location.longitude > mapBoundaries.northEast.lng ||
            location.latitude < mapBoundaries.southWest.lat || location.latitude > mapBoundaries.northEast.lat) {
            console.error("Location is out of map boundaries:", location);
            return;
        }
        let button = document.createElement('button');
        button.innerHTML = index + 1;
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
            if (panorama) {panorama.destroy()};
            panorama = pannellum.viewer('panorama', {
                "type": "equirectangular",
                "panorama": imageHostUrl+location.imageId + ".jpg",
                "autoLoad": true,
                "showControls": false,
            });
        });
        mapImage.parentElement.appendChild(button);
    });
};

function postImageData()
{
    let image_id = document.getElementById("image_id").value;
    let mapid = document.getElementById("coordinateMapId").value;
    let imageFile = document.getElementById("imageFile").files[0];
    let fetchUrl = apiHostUrl +"coordinates/1";

    EXIF.getData(imageFile, function() {
        let lat = EXIF.getTag(this, "GPSLatitude");
        let lng = EXIF.getTag(this, "GPSLongitude");
        let height = EXIF.getTag(this, "GPSAltitude");
        if (lat && lng && height) {
            lat = lat[0] + (lat[1] / 60) + (lat[2] / 3600);
            lng = lng[0] + (lng[1] / 60) + (lng[2] / 3600);
            height = parseFloat(height);
            
            fetch(fetchUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    id: 0,
                    imageId: image_id,
                    latitude: lat,
                    longitude: lng,
                    mapId: mapid,
                    height: height
                })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    console.log('Success:', data);
                })
                .catch(error => {
                    console.error('Error:', error);
                });

        } else {
            console.error("No GPS data found in image.");
        }
    });
}
function getMap() {
    let mapid = document.getElementById("mapId").value;
    let fetchUrl = apiHostUrl + "map/" + mapid;
    fetch(fetchUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        createMap(data);
        console.log('Success:', data);
        getMapData(mapid);
    })
    .catch(error => {
        console.error('Error:', error);
    });
}
function getMapData(mapid){
    let fetchUrl = apiHostUrl + "map/" + mapid + "/coordinates";
    fetch(fetchUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        return response.json();
    })
    .then(data => {
        updateMap(data);
        console.log('Success:', data);
    })
    .catch(error => {
        console.error('Error:', error);
    });
}