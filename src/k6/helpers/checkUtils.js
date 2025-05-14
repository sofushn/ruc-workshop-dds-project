export function isStatusCode(response, statusCode = 200) {
    return response.status === statusCode;
}

export function isResponseUrlList(response){
    try {
        const urls = JSON.parse(response.body);

        return Array.isArray(urls) && urls.length > 0 && urls.every(url => typeof url === "string" && url.startsWith("http"));
        
    } catch (error) {
        return false;
    }
}

export function isResponseImage(response) {
    const contentType = response.headers["Content-Type"];
    const contentLength = response.headers["Content-Length"];
    
    return (
        contentType &&
        contentType.startsWith("image/") &&
        contentLength > 0
    );
}