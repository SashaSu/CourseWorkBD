CREATE OR REPLACE FUNCTION get_all_warehouses()
    RETURNS TABLE (
                      id INT,
                      warehouse_type VARCHAR
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT id, warehouse_type
        FROM Warehouse;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_warehouse_by_id(p_id INT)
    RETURNS TABLE (
                      id INT,
                      warehouse_type VARCHAR
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT id, warehouse_type
        FROM Warehouse
        WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;
