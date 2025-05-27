CREATE OR REPLACE FUNCTION add_dish_to_order(p_order_id INT, p_dish_id INT)
    RETURNS VOID AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Orders WHERE id = p_order_id) THEN
        RAISE EXCEPTION 'Заказ с id % не найден', p_order_id;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM Dish WHERE id = p_dish_id) THEN
        RAISE EXCEPTION 'Блюдо с id % не найдено', p_dish_id;
    END IF;
    INSERT INTO Order_Dish(order_id, dish_id)
    VALUES (p_order_id, p_dish_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_dish_in_order(
    p_order_id INT,
    p_old_dish_id INT,
    p_new_dish_id INT
)
    RETURNS VOID AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Orders WHERE id = p_order_id) THEN
        RAISE EXCEPTION 'Заказ с id % не найден', p_order_id;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM Dish WHERE id = p_old_dish_id) THEN
        RAISE EXCEPTION 'Старое блюдо с id % не найдено', p_old_dish_id;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM Dish WHERE id = p_new_dish_id) THEN
        RAISE EXCEPTION 'Новое блюдо с id % не найдено', p_new_dish_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM Order_Dish
        WHERE order_id = p_order_id AND dish_id = p_old_dish_id
    ) THEN
        RAISE EXCEPTION 'Блюдо с id % не найдено в заказе %', p_old_dish_id, p_order_id;
    END IF;

    UPDATE Order_Dish
    SET dish_id = p_new_dish_id
    WHERE order_id = p_order_id AND dish_id = p_old_dish_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_dish_from_order(
    p_order_id INT,
    p_dish_id INT
)
    RETURNS VOID AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Orders WHERE id = p_order_id) THEN
        RAISE EXCEPTION 'Заказ с id % не найден', p_order_id;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM Dish WHERE id = p_dish_id) THEN
        RAISE EXCEPTION 'Блюдо с id % не найдено', p_dish_id;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM Order_Dish
        WHERE order_id = p_order_id AND dish_id = p_dish_id
    ) THEN
        RAISE EXCEPTION 'Блюдо с id % не найдено в заказе %', p_dish_id, p_order_id;
    END IF;

    DELETE FROM Order_Dish
    WHERE order_id = p_order_id AND dish_id = p_dish_id;
END;
$$ LANGUAGE plpgsql;
