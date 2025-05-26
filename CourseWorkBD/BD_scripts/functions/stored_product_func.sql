CREATE OR REPLACE FUNCTION update_stored_product_quantity_by_name(p_name VARCHAR, p_quantity FLOAT)
    RETURNS VOID AS $$
BEGIN
    UPDATE Stored_product
    SET quantity = p_quantity
    WHERE product_id IN (
        SELECT id FROM Product WHERE product_name = p_name
    );
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_stored_product_by_name(p_name VARCHAR)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM Stored_product
    WHERE product_id IN (
        SELECT id FROM Product WHERE product_name = p_name
    );
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION create_stored_product(
    p_warehouse_id INT,
    p_product_name VARCHAR,
    p_quantity FLOAT
)
    RETURNS VOID AS $$
DECLARE
    v_product_id INT;
BEGIN
    SELECT id INTO v_product_id
    FROM Product
    WHERE product_name = p_product_name;

    INSERT INTO Stored_product (warehouse_id, product_id, quantity)
    VALUES (p_warehouse_id, v_product_id, p_quantity);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_stored_products()
    RETURNS TABLE (
                      id INT,
                      warehouse_id INT,
                      product_name VARCHAR,
                      quantity FLOAT
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT sp.id, sp.warehouse_id, p.product_name, sp.quantity
        FROM Stored_product sp
                 JOIN Product p ON sp.product_id = p.id;
END;
$$ LANGUAGE plpgsql;
