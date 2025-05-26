-- просмотр инфы о продуктах
CREATE OR REPLACE FUNCTION get_all_products()
    RETURNS SETOF Product AS $$
SELECT * FROM Product;
$$ LANGUAGE sql;

-- изменениее статуса заказа
CREATE OR REPLACE FUNCTION update_order_status(
    new_id INT,
    new_status VARCHAR
)
    RETURNS VOID AS $$
BEGIN
    UPDATE Orders
    SET status = new_status
    WHERE id = new_id;
END;
$$ LANGUAGE plpgsql;

-- просмотр инфы о блюдах
CREATE OR REPLACE FUNCTION get_all_dishes()
    RETURNS SETOF Dish AS $$
SELECT * FROM Dish;
$$ LANGUAGE sql;
