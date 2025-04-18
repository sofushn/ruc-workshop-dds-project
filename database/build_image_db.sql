DROP TABLE IF EXISTS image;

create table image (
id INT IDENTITY PRIMARY KEY, 
x DECIMAL(8,4) not null,
y DECIMAL(8,4) not null,
reference_to_image_store varchar(255) not null,
relation_to_other_images varchar (255) not null
);


INSERT INTO image (x,y,reference_to_image_store,relation_to_other_images) VALUES 
(1201.2001,2354.4351,'test1','test1')

INSERT INTO image (x,y,reference_to_image_store,relation_to_other_images) VALUES 
(1201.2001,2354.4351,'test2','test2')

select* from image

