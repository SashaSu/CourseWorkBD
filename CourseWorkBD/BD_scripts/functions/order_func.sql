CREATE OR REPLACE FUNCTION create_order(p_table_id INT, p_status VARCHAR)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO Orders (table_id, status)
    VALUES (p_table_id, p_status);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_order(p_id INT, p_status VARCHAR)
    RETURNS VOID AS $$
BEGIN
    UPDATE Orders
    SET status = p_status
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_order(p_id INT)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM Orders
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_orders()
    RETURNS TABLE(id INT, table_id INT, time_order TIMESTAMP, status VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT * FROM Orders;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_order_by_id(p_id INT)
    RETURNS TABLE(id INT, table_id INT, time_order TIMESTAMP, status VARCHAR) AS $$
BEGIN
    RETURN QUERY
        SELECT * FROM Orders WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;
