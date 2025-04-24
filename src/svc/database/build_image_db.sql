DROP TABLE IF EXISTS image;


create table waypoint (
    id INT IDENTITY PRIMARY KEY, 
    longitude DECIMAL(15,14) NOT NULL UNIQUE,
    latitude DECIMAL(18,16) NOT NULL UNIQUE,
    height DECIMAL(4,2) NOT NULL,
    image_id VARCHAR(255) NOT NULL

);

create table map (
    id INT IDENTITY PRIMARY KEY,
    NW_longitude DECIMAL(15,14) NOT NULL UNIQUE,
    NW_latitude DECIMAL(18,16) NOT NULL UNIQUE,
    SE_longitude DECIMAL(15,14) NOT NULL UNIQUE,
    SE_latitude DECIMAL(18,16) NOT NULL UNIQUE,
    image_id VARCHAR(255) NOT NULL

)



INSERT INTO image (longitude,latitude,height,reference_to_image_store,relation_to_other_images) VALUES 
(1.20000000000001,23.4300000000000051,70.08,'test1','test1')

INSERT INTO image (longitude,latitude,height,reference_to_image_store,relation_to_other_images) VALUES 
(1.20000000000001,23.4300000000000051,50.56,'test2','test2')

INSERT INTO map (NW_longitude, NW_latitude, SE_longitude, SE_latitude, image_id) VALUES
(1.20000000000001,23.4300000000000051,9.40000000000002,73.5700000000000051)

--select* from image

