CREATE OR REPLACE FUNCTION create_employee(
    emp_name VARCHAR,
    emp_profession VARCHAR,
    emp_salary FLOAT
)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO Employee(name, profession, salary)
    VALUES (emp_name, emp_profession, emp_salary);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_employee(
    emp_id INT,
    emp_name VARCHAR,
    emp_salary FLOAT
)
    RETURNS VOID AS $$
BEGIN
    UPDATE Employee
    SET name = emp_name,
        salary = emp_salary
    WHERE id = emp_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_employees()
    RETURNS TABLE(id INT, name VARCHAR, profession VARCHAR, salary FLOAT) AS $$
BEGIN
    RETURN QUERY SELECT * FROM Employee;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_employee(lastname VARCHAR)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM Employee
    WHERE split_part(name, ' ', 1) ILIKE lastname;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION search_employees_by_lastname(lastname VARCHAR)
    RETURNS TABLE(id INT, name VARCHAR, profession VARCHAR, salary FLOAT) AS $$
BEGIN
    RETURN QUERY
        SELECT *
        FROM Employee
        WHERE split_part(name, ' ', 1) ILIKE lastname;
END;
$$ LANGUAGE plpgsql;
