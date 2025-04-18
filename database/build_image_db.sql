DROP TABLE IF EXISTS image;

create table image (
id INT PRIMARY KEY, 
x DECIMAL(38,10) not null,
y DECIMAL(38,10) not null,
reference_to_image_store varchar(255) not null,
relation_to_other_images varchar (255) not null
);


INSERT INTO image (id,x,y,reference_to_image_store,relation_to_other_images) VALUES 
(1,1201.2001,2354.4351,'test1','test1')