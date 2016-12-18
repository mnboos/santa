delete from gifts_grouped;
select groupgifts();
-- select * from gifts_grouped order by longitude;
delete from gifts_solution;
select calculateTiredness();
select sum(tiredness) from gifts_solution;