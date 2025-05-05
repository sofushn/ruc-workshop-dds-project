DROP TABLE IF EXISTS image;

create table map (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    ne_longitude DECIMAL(18,15) NOT NULL UNIQUE,
    ne_latitude DECIMAL(18,15) NOT NULL UNIQUE,
    sw_longitude DECIMAL(18,15) NOT NULL UNIQUE,
    sw_latitude DECIMAL(18,15) NOT NULL UNIQUE,
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



INSERT INTO map (ne_longitude, ne_latitude, sw_longitude, sw_latitude, image_id) VALUES
(12.144353,55.655294,12.134139,55.651107,'test1');

INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (12.140358,55.653372,70.08,'1',1);

INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (12.134499,55.651459,50.56,'2',1);

 INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (12.139993,55.652658,50.56,'3',1);

 INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (12.138877,55.654111,50.56,'4',1);