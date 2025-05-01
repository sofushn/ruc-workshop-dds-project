let imageHostUrl = "http://localhost:8080/images/";
let apiHostUrl = "http://localhost:5165/coordinates/";
let mapImage = document.getElementById('mapImage');
mapImage.src = imageHostUrl+"2dmap.jpg";

let mapBoundaries = {
    northEast: { lat: 55.655294, lng: 12.144353 },
    southWest: { lat: 55.651107, lng: 12.134139 },
};

let panorama = pannellum.viewer('panorama', {
    "type": "equirectangular",
    "autoLoad": true,
    "showControls": false,
});

function updateMap(data) {
    data.forEach((location, index) => {
        let button = document.createElement('button');
        button.innerHTML = index + 1;
        button.style.position = 'absolute';
        button.style.width = '10px';
        button.style.height = '10px';
        button.style.backgroundColor = 'red';
        button.style.borderRadius = '50%';
        button.style.left = ((location.x - mapBoundaries.southWest.lng) / 
                            (mapBoundaries.northEast.lng - mapBoundaries.southWest.lng) * 100) + '%';
        button.style.top = ((mapBoundaries.northEast.lat - location.y) / 
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
    let lat = document.getElementById("latitude").value;
    let lng = document.getElementById("longitude").value;
    let image_id = document.getElementById("image_id").value;
    fetch(apiHostUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            imageId: image_id,
            x: parseFloat(lng),
            y: parseFloat(lat)
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
}
function getImageData(){
    fetch(apiHostUrl, {
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

function postPlaceholderData(){
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
    locationData.forEach(location => {
        fetch(apiHostUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                imageId: location.image_id,
                x: parseFloat(location.longitude),
                y: parseFloat(location.latitude)
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
    });
}