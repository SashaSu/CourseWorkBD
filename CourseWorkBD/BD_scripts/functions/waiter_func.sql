CREATE OR REPLACE FUNCTION get_all_tables()
    RETURNS SETOF Tables AS $$
SELECT * FROM Tables;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION create_order(
    new_table_id INT,
    new_status VARCHAR
)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO Orders(table_id, status)
    VALUES (new_table_id, new_status);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_order_status(
    new_id INT,
    new_status VARCHAR
)
    RETURNS VOID AS $$
BEGIN
    UPDATE Orders
    SET status = new_status
    WHERE id = new_id;
END;
$$ LANGUAGE plpgsql;
