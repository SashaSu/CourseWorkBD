CREATE OR REPLACE FUNCTION get_all_dishes()
    RETURNS TABLE (
                      id INT,
                      dish_name VARCHAR,
                      price FLOAT,
                      calories INT,
                      weight INT
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT d.id, d.dish_name, d.price, d.calories, d.weight
        FROM Dish d;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION find_dishes_by_name(search_term VARCHAR)
    RETURNS TABLE (
                      id INT,
                      dish_name VARCHAR,
                      price FLOAT,
                      calories INT,
                      weight INT
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT id, dish_name, price, calories, weight
        FROM Dish
        WHERE dish_name ILIKE '%' || search_term || '%';
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_dishes_in_order(p_order_id INT)
    RETURNS TABLE (
                      order_id INT,
                      dish_id INT,
                      dish_name VARCHAR
                  ) AS $$
BEGIN
    RETURN QUERY
        SELECT p_order_id, d.id, d.dish_name
        FROM Order_Dish od
                 JOIN Dish d ON od.dish_id = d.id
        WHERE od.order_id = p_order_id;
END;
$$ LANGUAGE plpgsql;
