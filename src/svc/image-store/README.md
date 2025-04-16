
# Image Store API - Static Image Service

**The place were all images are stored and served from.**

## Overview
`Image Store API` is built with the ASP.NET Core Web API, following the minimal API convention.
Its purpose is to provide access to the stored images for other services. 

## Features
- **📂 Static File Serving:** Serves images from the local `Images` folder.
- **🔗 Image Listing Endpoint:** Provides a list of all avaible image URLs.
- **🐶 Custom Error Pages:** Displays custom error page, with dummy picture. Currenly only supports 404, showing dog image from https://http.dog/404.jpg 
- **📘 Swagger Integration:** Uses standard configuration of Swagger UI for API documentation & testing

## API Reference

#### Get all images

```https
  GET /images
```

#### Get item

```https
  GET /images/IMG_ID
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `IMG_id`      | `string` | **Optional**. Id of specific img to fetch |
