SELECT * FROM Employee;
SELECT * FROM Product;
SELECT * FROM Warehouse;
SELECT * FROM Tables;
SELECT * FROM Dish;
SELECT * FROM Orders;
SELECT * FROM Stored_product;
SELECT * FROM Dish_Product;
SELECT * FROM Order_Dish;
SELECT * FROM Employee_Order;
SELECT * FROM Account;

SELECT MAX(id) FROM Employee;
SELECT nextval('employee_id_seq');

SELECT MAX(id) FROM account;
SELECT nextval('account_id_seq');

DO $$
    DECLARE
        max_id INT;
        seq_name TEXT := 'employee_id_seq';
    BEGIN
        -- Получаем максимальный id из таблицы Employee
        SELECT MAX(id) INTO max_id FROM Employee;

        IF max_id IS NOT NULL THEN
            -- Устанавливаем значение последовательности на max_id
            EXECUTE format('SELECT setval(%L, %s, true)', seq_name, max_id);
        END IF;
    END;
$$;

DO $$
    DECLARE
        max_id INT;
        seq_name TEXT := 'account_id_seq';
    BEGIN
        -- Получаем максимальный id из таблицы Account
        SELECT MAX(id) INTO max_id FROM Account;

        IF max_id IS NOT NULL THEN
            -- Устанавливаем значение последовательности на max_id
            EXECUTE format('SELECT setval(%L, %s, true)', seq_name, max_id);
        END IF;
    END;
$$;
