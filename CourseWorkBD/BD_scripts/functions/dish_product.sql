CREATE OR REPLACE FUNCTION get_products_in_dish(p_dish_id INT)
    RETURNS TABLE (
                      product_id INT,
                      product_name VARCHAR
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT
            p.id,
            p.product_name
        FROM Dish_Product dp
                 JOIN Product p ON dp.product_id = p.id
        WHERE dp.dish_id = p_dish_id;
END;
$$ LANGUAGE plpgsql;
