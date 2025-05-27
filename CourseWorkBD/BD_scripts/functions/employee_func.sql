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

CREATE OR REPLACE FUNCTION delete_employee(emp_id INT)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM Employee e
    WHERE e.id = emp_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION search_employee_by_id(emp_id INT)
    RETURNS TABLE(id INT, name VARCHAR, profession VARCHAR, salary FLOAT) AS $$
BEGIN
    RETURN QUERY
        SELECT e.id, e.name, e.profession, e.salary
        FROM Employee e
        WHERE e.id = emp_id;
END;
$$ LANGUAGE plpgsql;

