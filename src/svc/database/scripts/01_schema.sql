CREATE TABLE map (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    ne_longitude DECIMAL(18,15) NOT NULL,
    ne_latitude DECIMAL(18,15) NOT NULL,
    sw_longitude DECIMAL(18,15) NOT NULL,
    sw_latitude DECIMAL(18,15) NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    UNIQUE (ne_longitude, ne_latitude, sw_longitude, sw_latitude)
);

CREATE TABLE waypoint (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY, 
    longitude DECIMAL(18,15) NOT NULL,
    latitude DECIMAL(18,15) NOT NULL,
    height DECIMAL(4,2) NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    map_id INT REFERENCES map(id) NOT NULL,
    UNIQUE (longitude, latitude)
);
