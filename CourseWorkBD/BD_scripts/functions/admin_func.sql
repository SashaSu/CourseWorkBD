-- вывести всех сотрудников
CREATE OR REPLACE FUNCTION get_all_employees()
    RETURNS SETOF Employee
AS $$
    BEGIN 
SELECT * FROM Employee;
END;
$$ LANGUAGE plpgsql;


-- добавить сотрудника
CREATE OR REPLACE FUNCTION add_employee(
    new_profession VARCHAR(256)
)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO Employee (profession)
    VALUES (new_profession);
END;
$$ LANGUAGE plpgsql;

--  изменить информацию о сотруднике
CREATE OR REPLACE FUNCTION update_employee(
    e_id SERIAL,
    new_profession VARCHAR(256)
)
    RETURNS VOID AS $$
BEGIN
    UPDATE Employee
    SET
        profession = new_profession
    WHERE id = e_id;
END;
$$ LANGUAGE plpgsql;

-- просмотр инфы о заказах
CREATE OR REPLACE FUNCTION get_all_orders()
    RETURNS SETOF Orders
AS $$
BEGIN
    SELECT * FROM Orders;
END;
$$ LANGUAGE plpgsql;

-- редактирование инфы о столах
CREATE OR REPLACE FUNCTION update_table(
    t_id SERIAL,
    new_location VARCHAR(256)
)
    RETURNS VOID AS $$
BEGIN
    UPDATE Tables
    SET
        location = new_location
    WHERE id = t_id;
END;
$$ LANGUAGE plpgsql;

-- просмотр инфы о столиках
CREATE OR REPLACE FUNCTION get_all_tables()
    RETURNS SETOF Tables
AS $$
BEGIN
    SELECT * FROM Tables;
END;
$$ LANGUAGE plpgsql;


