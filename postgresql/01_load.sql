drop table gifts;
CREATE TABLE gifts ( 
	gift_id int PRIMARY KEY,
    latitude float8 NOT NULL,
    longitude float8 NOT NULL,
    weight float8 NOT NULL
);

COPY gifts FROM 'E:\Workspace\santa\gifts.csv' DELIMITER ';' CSV;

DROP TABLE gifts_grouped;
CREATE TABLE gifts_grouped ( 
	gift_id int PRIMARY KEY,
    group_id int NOT NULL,
    acc_weigth real NOT NULL,
    latitude float8 NOT NULL,
    longitude float8 NOT NULL,
    weight float8 NOT NULL
);

DROP TABLE gifts_solution;
CREATE TABLE gifts_solution ( 
	gift_id int PRIMARY KEY,
    group_id int NOT NULL,
    latitude float8 NOT NULL,
    longitude float8 NOT NULL,
    weight float8 NOT NULL,
    remaining_weigth float8,
    tiredness float8
);
