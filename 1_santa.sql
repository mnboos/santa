drop table landmarks;

CREATE TABLE landmarks

(

gid serial NOT NULL,

giftid numeric(12,0),

weight numeric(12,8),

landmark character varying(10),

latitude numeric(12,8),

longitude numeric(12,8),

the_geom geometry,

CONSTRAINT landmarks_pkey PRIMARY KEY (gid),

CONSTRAINT enforce_dims_the_geom CHECK (st_ndims(the_geom) = 2),

CONSTRAINT enforce_geotype_geom CHECK (geometrytype(the_geom) = 'POINT'::text OR the_geom IS NULL),

CONSTRAINT enforce_srid_the_geom CHECK (st_srid(the_geom) = 4326)

);