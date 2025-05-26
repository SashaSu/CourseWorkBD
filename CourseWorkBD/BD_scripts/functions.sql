CREATE OR REPLACE FUNCTION get_all_employees()
    RETURNS SETOF Employee
AS $$
SELECT * FROM Employee;
$$ LANGUAGE sql;

SELECT * FROM get_all_employees();


