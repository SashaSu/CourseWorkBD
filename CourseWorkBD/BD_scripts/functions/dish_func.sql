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
        SELECT id, dish_name, price, calories, weight
        FROM Dish;
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
