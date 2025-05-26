CREATE OR REPLACE FUNCTION add_product(p_name VARCHAR, p_type VARCHAR)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO Product (product_name, product_type)
    VALUES (p_name, p_type);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_product_by_name(p_old_name VARCHAR, p_new_name VARCHAR, p_new_type VARCHAR)
    RETURNS VOID AS $$
BEGIN
    UPDATE Product
    SET product_name = p_new_name,
        product_type = p_new_type
    WHERE product_name = p_old_name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_product_by_name(p_name VARCHAR)
    RETURNS VOID AS $$
BEGIN
    DELETE FROM Product
    WHERE product_name = p_name;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_products()
    RETURNS TABLE(id INT, product_name VARCHAR, product_type VARCHAR) AS $$
BEGIN
    RETURN QUERY SELECT * FROM Product;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION find_products_by_name(search_term VARCHAR)
    RETURNS TABLE(id INT, product_name VARCHAR, product_type VARCHAR) AS $$
BEGIN
    RETURN QUERY
        SELECT *
        FROM Product
        WHERE product_name ILIKE '%' || search_term || '%';
END;
$$ LANGUAGE plpgsql;
