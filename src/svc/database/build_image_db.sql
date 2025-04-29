DROP TABLE IF EXISTS image;

create table map (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    NE_longitude DECIMAL(15,14) NOT NULL UNIQUE,
    NE_latitude DECIMAL(18,16) NOT NULL UNIQUE,
    SW_longitude DECIMAL(15,14) NOT NULL UNIQUE,
    SW_latitude DECIMAL(18,16) NOT NULL UNIQUE,
    image_id VARCHAR(255) NOT NULL

);

create table waypoint (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY, 
    longitude DECIMAL(15,14) NOT NULL UNIQUE,
    latitude DECIMAL(18,16) NOT NULL UNIQUE,
    height DECIMAL(4,2) NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    map_id INT REFERENCES map(id) NOT NULL

);



INSERT INTO map (NW_longitude, NW_latitude, SE_longitude, SE_latitude, image_id) VALUES
(1.20000000000001,23.4300000000000051,9.40000000000002,73.5700000000000051,'test1');

INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (1.20000000000011,23.4300000000000321,70.08,'test1',1);

INSERT INTO waypoint (longitude,latitude,height,image_id,map_id) VALUES 
 (1.20000000000001,23.4300000000000051,50.56,'test2',1);




