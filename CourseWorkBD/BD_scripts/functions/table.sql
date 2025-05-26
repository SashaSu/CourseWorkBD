CREATE OR REPLACE FUNCTION get_all_tables()
    RETURNS TABLE (
                      id INT,
                      location VARCHAR,
                      status VARCHAR
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT id, location, status
        FROM Tables;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_table_status(p_id INT, p_status VARCHAR)
    RETURNS VOID AS $$
BEGIN
    UPDATE Tables
    SET status = p_status
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;
