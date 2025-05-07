DROP TABLE IF EXISTS image;

create table map (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    NE_longitude DECIMAL(18,15) NOT NULL UNIQUE,
    NE_latitude DECIMAL(18,15) NOT NULL UNIQUE,
    SW_longitude DECIMAL(18,15) NOT NULL UNIQUE,
    SW_latitude DECIMAL(18,15) NOT NULL UNIQUE,
    image_id VARCHAR(255) NOT NULL

);

create table waypoint (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY, 
    longitude DECIMAL(18,15) NOT NULL UNIQUE,
    latitude DECIMAL(18,15) NOT NULL UNIQUE,
    height DECIMAL(4,2) NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    map_id INT REFERENCES map(id) NOT NULL

);



INSERT INTO map (NE_longitude, NE_latitude, SW_longitude, SW_latitude, image_id) VALUES
(12.144353,55.655294,12.134139,55.651107,'test1');

INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (12.140358,55.653372,70.08,'1',1);

INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (12.134499,55.651459,50.56,'2',1);




