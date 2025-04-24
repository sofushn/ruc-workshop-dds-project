DROP TABLE IF EXISTS image;


create table image (
id INT IDENTITY PRIMARY KEY, 
longitude DECIMAL(15,14) not null,
latitude DECIMAL(18,16) not null,
height DECIMAL(4,2) not null,
reference_to_image_store varchar(255) not null,
relation_to_other_images varchar (255) not null
);


INSERT INTO image (longitude,latitude,height,reference_to_image_store,relation_to_other_images) VALUES 
(1.20000000000001,23.4300000000000051,70.08,'test1','test1')

INSERT INTO image (longitude,latitude,height,reference_to_image_store,relation_to_other_images) VALUES 
(1.20000000000001,23.4300000000000051,50.56,'test2','test2')

--select* from image

