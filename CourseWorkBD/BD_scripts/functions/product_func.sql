CREATE OR REPLACE FUNCTION insert_product(p_name VARCHAR, p_type VARCHAR)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO product(product_name, product_type) VALUES (p_name, p_type);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_product(p_id INT, p_name VARCHAR, p_type VARCHAR)
    RETURNS VOID AS $$
BEGIN
    UPDATE product SET product_name = p_name, product_type = p_type WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_product(p_id INT)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM product WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_products()
    RETURNS TABLE(id INT, product_name VARCHAR, product_type VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT * FROM Product;
END;
$$ LANGUAGE plpgsql;


