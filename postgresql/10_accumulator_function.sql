-- ----------------------------------------
-- Groups the Gifts according to the Longitute
-- Imagine it as sweep line algorithm
-- ----------------------------------------
CREATE OR REPLACE FUNCTION groupGifts() RETURNS REAL AS $$
DECLARE
	cur_gift CURSOR FOR SELECT * FROM gifts order by longitude;
    rec_gift RECORD;
    acc REAL;
    acc_l REAL;
    grp int;
BEGIN
	OPEN cur_gift;
    acc   := 0;
    acc_l := 0;
    grp := 0;
    LOOP
    	FETCH cur_gift INTO rec_gift; -- Get next line
        EXIT WHEN NOT FOUND; -- All Processed
        
        acc := acc + rec_gift.weight;
        grp := floor(acc/1000);
        
        -- IF acc is passing 1000, start at the next 1000
        IF grp>floor(acc_l/1000) THEN 
        	acc := grp*1000 + rec_gift.weight;
        END IF;
        
        INSERT INTO gifts_grouped
        	VALUES (rec_gift.gift_id,grp, acc, rec_gift.latitude, rec_gift.longitude, rec_gift.weight);
        acc_l := acc;
        
    END LOOP;
    
    RETURN acc;
END
$$ LANGUAGE plpgsql;


-- Copied from https://gist.github.com/carlzulauf/1724506
-- Haversine Formula based geodistance in miles (constant is diameter of Earth in miles)
-- Based on a similar PostgreSQL function found here: https://gist.github.com/831833
-- Updated to use distance formulas found here: http://www.codecodex.com/wiki/Calculate_distance_between_two_points_on_a_globe
CREATE OR REPLACE FUNCTION public.geodistance(alat double precision, alng double precision, blat double precision, blng double precision)
  RETURNS double precision AS
$BODY$
SELECT asin(
  sqrt(
    sin(radians($3-$1)/2)^2 +
    sin(radians($4-$2)/2)^2 *
    cos(radians($1)) *
    cos(radians($3))
  )
) * 7926.3352 AS distance;
$BODY$
  LANGUAGE sql IMMUTABLE
  COST 100;

CREATE OR REPLACE FUNCTION calculateTiredness() RETURNS REAL AS $$
DECLARE
	cur CURSOR FOR SELECT 
    	a.gift_id, 
        a.group_id, 
        a.latitude, 
        a.longitude,
        a.weight,
        b.total_load
    	FROM gifts_grouped a, (SELECT group_id, max(acc_weigth)-group_id*1000 total_load FROM gifts_grouped GROUP BY group_id) b
        WHERE a.group_id = b.group_id
        ORDER BY group_id asc, latitude desc;
    rec RECORD;
    -- rec_l RECROD;
	grp int;
    grp_l int;
    acc real;
    tired real;
    tired_acc real;
    lati_l float8;
    long_l float8;
BEGIN
	OPEN cur;
    grp_l := -1;
    lati_l := 0;
    long_l := 0;
    
    LOOP
        FETCH cur INTO rec;
        EXIT WHEN NOT FOUND;

        grp = rec.group_id;
        IF grp_l<grp THEN
            tired := geodistance(lati_l, long_l, 0, 0)*10;
        	-- Return to the North Pole
            INSERT INTO gifts_solution VALUES
            	(grp*-1, grp, 0, 0, 0, acc, tired);
            tired_acc := tired_acc+tired;
            -- Initialize the group
            lati_l := 0;
    		long_l := 0;
            acc := rec.total_load+10; -- +10 for the Sleigh
            grp_l := grp;
        END IF;
    	
        tired := geodistance(lati_l, long_l, rec.latitude, rec.longitude)*acc;
        INSERT INTO gifts_solution VALUES
        	(rec.gift_id, grp, rec.latitude, rec.longitude, rec.weight, acc, tired);
        tired_acc := tired_acc+tired;
		acc := acc - rec.weight;
        lati_l := rec.latitude;
    	long_l := rec.longitude;
        
    END LOOP;
    RETURN tired_acc;
END
$$ LANGUAGE plpgsql;
