let imageHostUrl = "http://localhost:8080/images/";

export async function fetchImage(image_id) {
    const response = await fetch(`${imageHostUrl}${image_id}`)
    if (!response.ok) {
        throw new Error('Network response was not ok' + response.statusText);
    }
    let imageUrl = await response.json();
    return imageUrl;
}