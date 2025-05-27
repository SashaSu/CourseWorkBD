CREATE OR REPLACE FUNCTION login(username VARCHAR(256), password VARCHAR(256))
    RETURNS VARCHAR(256)
    LANGUAGE plpgsql
    SECURITY DEFINER
AS $$
DECLARE
    prof VARCHAR(256);
BEGIN
    SELECT e.profession
    INTO prof
    FROM Account a
             JOIN Employee e ON e.id = a.employee_id
    WHERE a.username = login.username AND a.password = login.password;

    RETURN prof;
END;
$$;


CREATE OR REPLACE FUNCTION get_all_accounts()
    RETURNS TABLE(account_id INT, acc_username VARCHAR, acc_employee_id INT) AS $$
BEGIN
    RETURN QUERY
        SELECT a.id AS account_id, a.username AS acc_username, a.employee_id AS acc_employee_id
        FROM Account a;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION create_account(
    p_username VARCHAR,
    p_password VARCHAR,
    p_employee_id INT
)
    RETURNS VOID
    LANGUAGE plpgsql
    SECURITY DEFINER
AS $$
DECLARE
    v_position TEXT;
    v_role TEXT;
BEGIN

    SELECT profession INTO v_position
    FROM employee
    WHERE id = p_employee_id;

    IF v_position IS NULL THEN
        RAISE EXCEPTION 'Сотрудник с id % не найден', p_employee_id;
    END IF;

    CASE v_position
        WHEN 'Администратор' THEN v_role := 'admin_role';
        WHEN 'Повар' THEN v_role := 'cook_role';
        WHEN 'Официант' THEN v_role := 'waiter_role';
        WHEN 'Кладовщик' THEN v_role := 'warehouse_manager_role';
        ELSE
            RAISE EXCEPTION 'Неизвестная должность: %', v_position;
        END CASE;

    EXECUTE format('CREATE USER %I WITH PASSWORD %L', p_username, p_password);

    EXECUTE format('GRANT %I TO %I', v_role, p_username);

    INSERT INTO Account(username, password, employee_id)
    VALUES (p_username, p_password, p_employee_id);
END;
$$;

CREATE OR REPLACE FUNCTION update_account(
    p_id INT,
    p_username VARCHAR,
    p_password VARCHAR,
    p_employee_id INT
)
    RETURNS VOID
    LANGUAGE plpgsql
    SECURITY DEFINER
AS $$
DECLARE
    v_old_username TEXT;
    v_old_role TEXT;
    v_new_role TEXT;
    v_old_position TEXT;
    v_new_position TEXT;
BEGIN
    -- Старое имя пользователя
    SELECT username INTO v_old_username
    FROM Account
    WHERE id = p_id;

    IF v_old_username IS NULL THEN
        RAISE EXCEPTION 'Аккаунт с ID % не найден', p_id;
    END IF;

    -- Тек. профессия
    SELECT profession INTO v_new_position
    FROM employee
    WHERE id = p_employee_id;

    IF v_new_position IS NULL THEN
        RAISE EXCEPTION 'Сотрудник с ID % не найден', p_employee_id;
    END IF;

    -- Роль по новой профессии
    CASE v_new_position
        WHEN 'Администратор' THEN v_new_role := 'admin_role';
        WHEN 'Повар' THEN v_new_role := 'cook_role';
        WHEN 'Официант' THEN v_new_role := 'waiter_role';
        WHEN 'Кладовщик' THEN v_new_role := 'warehouse_manager_role';
        ELSE
            RAISE EXCEPTION 'Неизвестная профессия: %', v_new_position;
        END CASE;

    -- Удаляем старого пользователя 
    IF v_old_username <> p_username THEN
        EXECUTE format('DROP USER IF EXISTS %I', v_old_username);
        EXECUTE format('CREATE USER %I WITH PASSWORD %L', p_username, p_password);
        EXECUTE format('GRANT %I TO %I', v_new_role, p_username);
    ELSE
        -- Только пароль и роль 
        EXECUTE format('ALTER USER %I WITH PASSWORD %L', p_username, p_password);
        EXECUTE format('REVOKE ALL ON SCHEMA public FROM %I', p_username);
        EXECUTE format('GRANT %I TO %I', v_new_role, p_username);
    END IF;

    -- Обновление
    UPDATE Account
    SET username = p_username,
        password = p_password,
        employee_id = p_employee_id
    WHERE id = p_id;
END;
$$;


CREATE OR REPLACE FUNCTION delete_account(p_id INT)
    RETURNS VOID
    LANGUAGE plpgsql
    SECURITY DEFINER
AS $$
DECLARE
    v_username TEXT;
BEGIN
    SELECT username INTO v_username
    FROM Account
    WHERE id = p_id;

    IF v_username IS NULL THEN
        RAISE EXCEPTION 'Аккаунт с ID % не найден', p_id;
    END IF;

    EXECUTE format('DROP USER IF EXISTS %I', v_username);

    DELETE FROM Account
    WHERE id = p_id;
END;
$$;


