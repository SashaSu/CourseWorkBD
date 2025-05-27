CREATE OR REPLACE FUNCTION get_all_stored_products()
    RETURNS TABLE (
                      id INT,
                      warehouse_id INT,
                      product_id INT,
                      quantity FLOAT
                  ) AS $$
BEGIN
    RETURN QUERY SELECT * FROM Stored_product;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION add_stored_product(
    p_warehouse_id INT,
    p_product_id INT,
    p_quantity FLOAT
) RETURNS VOID AS $$
BEGIN
    IF p_quantity < 0 THEN
        RAISE EXCEPTION 'Количество не может быть отрицательным';
    END IF;

    INSERT INTO Stored_product(warehouse_id, product_id, quantity)
    VALUES (p_warehouse_id, p_product_id, p_quantity);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_stored_product(
    p_id INT,
    p_warehouse_id INT,
    p_product_id INT,
    p_quantity FLOAT
) RETURNS VOID AS $$
BEGIN
    IF p_quantity < 0 THEN
        RAISE EXCEPTION 'Количество не может быть отрицательным';
    END IF;

    UPDATE Stored_product
    SET warehouse_id = p_warehouse_id,
        product_id = p_product_id,
        quantity = p_quantity
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_stored_product(p_id INT)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM Stored_product WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_warehouse_type_by_stored_product_id(sp_id INT)
    RETURNS TABLE (stored_product_id INT, warehouse_type VARCHAR) AS $$
BEGIN
    RETURN QUERY
        SELECT sp.id, w.warehouse_type
        FROM stored_product sp
                 JOIN warehouse w ON sp.warehouse_id = w.id
        WHERE sp.id = sp_id;
END;
$$ LANGUAGE plpgsql;