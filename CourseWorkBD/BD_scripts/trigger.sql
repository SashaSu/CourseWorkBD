CREATE OR REPLACE FUNCTION update_products_on_order()
    RETURNS TRIGGER AS $$
DECLARE
    rec RECORD;
    available_qty FLOAT;
    stored_id INT;
BEGIN
    FOR rec IN
        SELECT dp.product_id, dp.quantity AS required_qty
        FROM Dish_Product dp
        WHERE dp.dish_id = NEW.dish_id
        LOOP
            SELECT id, quantity INTO stored_id, available_qty
            FROM Stored_Product
            WHERE product_id = rec.product_id
            ORDER BY quantity DESC
            LIMIT 1;

            IF stored_id IS NULL THEN
                RAISE EXCEPTION 'Продукт % не найден на складе.', rec.product_id;
            END IF;

            IF available_qty < rec.required_qty THEN
                RAISE EXCEPTION 'Недостаточно продукта % на складе. Требуется: %, доступно: %.',
                    rec.product_id, rec.required_qty, available_qty;
            END IF;

            UPDATE Stored_Product
            SET quantity = quantity - rec.required_qty
            WHERE id = stored_id;
        END LOOP;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_products_on_order
    AFTER INSERT ON Order_Dish
    FOR EACH ROW
EXECUTE FUNCTION update_products_on_order();
