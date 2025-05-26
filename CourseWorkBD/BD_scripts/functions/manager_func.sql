-- просмотр всех продуктов
CREATE OR REPLACE FUNCTION get_all_products()
    RETURNS SETOF Product AS $$
SELECT * FROM Product;
$$ LANGUAGE sql;

-- добавление продукта
CREATE OR REPLACE FUNCTION add_product(
    new_product_name VARCHAR,
    new_product_type VARCHAR
)
    RETURNS VOID AS $$
BEGIN
    INSERT INTO Product(product_name, product_type)
    VALUES (new_product_name, new_product_type);
END;
$$ LANGUAGE plpgsql;

-- обновить инфу о продукте
CREATE OR REPLACE FUNCTION update_product(
    new_id INT,
    new_product_name VARCHAR,
    new_product_type VARCHAR
)
    RETURNS VOID AS $$
BEGIN
    UPDATE Product
    SET
        product_name = new_product_name,
        product_type = new_product_type
    WHERE id = new_id;
END;
$$ LANGUAGE plpgsql;
