-- TODO поменять ALTER на CHECK
-- Ограничения для таблицы Employee
ALTER TABLE Employee
    ALTER COLUMN profession SET NOT NULL,
    ADD CONSTRAINT profession_check CHECK (profession IN ('Официант', 'Повар', 'Администратор', 'Кладовщик')),
    ADD CONSTRAINT salary_check CHECK (salary >= 22440),
    ADD CONSTRAINT name_word_count CHECK (
        array_length(regexp_split_to_array(name, '\s+'), 1) >= 3
        );

-- Ограничения для таблицы Orders
ALTER TABLE Orders
    ALTER COLUMN table_id SET NOT NULL,
    ALTER COLUMN time_order SET NOT NULL,
    ALTER COLUMN status SET NOT NULL,
    ADD CONSTRAINT status_check CHECK (status IN ('Принят', 'Готовится', 'Готов', 'Выдан', 'Оплачен')),
    ADD CONSTRAINT time_order_check CHECK (time_order <= NOW());

ALTER TABLE Orders
    DROP CONSTRAINT IF EXISTS fk_orders_table;
ALTER TABLE Orders
    ADD CONSTRAINT fk_orders_table FOREIGN KEY (table_id) REFERENCES Tables(id) ON DELETE CASCADE;

-- Ограничения для таблицы Tables
ALTER TABLE Tables
    ALTER COLUMN location SET NOT NULL,
    ADD CONSTRAINT location_check CHECK (location IN ('Зал', 'Терраса')),
    ADD CONSTRAINT status_check CHECK (status IN ('Занят', 'Свободен'));

-- Ограничения для таблицы Dish
ALTER TABLE Dish
    ALTER COLUMN dish_name SET NOT NULL,
    ALTER COLUMN calories SET NOT NULL,
    ALTER COLUMN weight SET NOT NULL;

ALTER TABLE Dish
    DROP CONSTRAINT IF EXISTS calories_check;
ALTER TABLE Dish
    ADD CONSTRAINT calories_check CHECK (calories >= 100 AND calories <= 2000);
ALTER TABLE Dish
    DROP CONSTRAINT IF EXISTS weight_check;
ALTER TABLE Dish
    ADD CONSTRAINT weight_check CHECK (weight >= 100 AND weight <= 1500);

-- Ограничения для таблицы Product
ALTER TABLE Product
    ALTER COLUMN product_name SET NOT NULL,
    ALTER COLUMN product_type SET NOT NULL;

-- Ограничения для таблицы Warehouse
ALTER TABLE Warehouse
    ALTER COLUMN warehouse_type SET NOT NULL,
    ADD CONSTRAINT warehouse_type_check CHECK (warehouse_type IN ('Обычный', 'Холодный'));

-- Ограничения для таблицы Stored_product
ALTER TABLE Stored_product
    ALTER COLUMN warehouse_id SET NOT NULL,
    ALTER COLUMN product_id SET NOT NULL,
    ADD CONSTRAINT quantity_check CHECK (quantity >= 0);

ALTER TABLE Stored_product
    DROP CONSTRAINT IF EXISTS fk_stored_product_warehouse;
ALTER TABLE Stored_product
    DROP CONSTRAINT IF EXISTS fk_stored_product_product;
ALTER TABLE Stored_product
    ADD CONSTRAINT fk_stored_product_warehouse FOREIGN KEY (warehouse_id) REFERENCES Warehouse(id) ON DELETE CASCADE;
ALTER TABLE Stored_product
    ADD CONSTRAINT fk_stored_product_product FOREIGN KEY (product_id) REFERENCES Product(id) ON DELETE CASCADE;

-- Ограничения для таблицы Employee_Order
ALTER TABLE Employee_Order
    DROP CONSTRAINT IF EXISTS fk_employee_order_employee;
ALTER TABLE Employee_Order
    DROP CONSTRAINT IF EXISTS fk_employee_order_order;
ALTER TABLE Employee_Order
    ADD CONSTRAINT fk_employee_order_employee FOREIGN KEY (employee_id) REFERENCES Employee(id) ON DELETE CASCADE;
ALTER TABLE Employee_Order
    ADD CONSTRAINT fk_employee_order_order FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE CASCADE,
    ADD CONSTRAINT unique_employee_order UNIQUE (employee_id, order_id);

-- Ограничения для таблицы Order_Dish
ALTER TABLE Order_Dish
    DROP CONSTRAINT IF EXISTS fk_order_dish_order;
ALTER TABLE Order_Dish
    DROP CONSTRAINT IF EXISTS fk_order_dish_dish;
ALTER TABLE Order_Dish
    ADD CONSTRAINT fk_order_dish_order FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE CASCADE;
ALTER TABLE Order_Dish
    ADD CONSTRAINT fk_order_dish_dish FOREIGN KEY (dish_id) REFERENCES Dish(id) ON DELETE CASCADE;

-- Ограничения для таблицы Dish_Product
ALTER TABLE Dish_Product
    DROP CONSTRAINT IF EXISTS fk_dish_product_dish;
ALTER TABLE Dish_Product
    DROP CONSTRAINT IF EXISTS fk_dish_product_product;
ALTER TABLE Dish_Product
    ADD CONSTRAINT fk_dish_product_dish FOREIGN KEY (dish_id) REFERENCES Dish(id) ON DELETE CASCADE,
    ADD CONSTRAINT quantity_check CHECK (quantity >= 0);
ALTER TABLE Dish_Product
    ADD CONSTRAINT fk_dish_product_product FOREIGN KEY (product_id) REFERENCES Product(id) ON DELETE CASCADE;

-- Ограничения для таблицы Account
ALTER TABLE Account
    ALTER COLUMN username SET NOT NULL,
    ALTER COLUMN password SET NOT NULL,
    ADD CONSTRAINT unique_username UNIQUE (username);
ALTER TABLE Account
    DROP CONSTRAINT IF EXISTS fk_account_employee;
ALTER TABLE Account
    ADD CONSTRAINT fk_account_employee FOREIGN KEY (employee_id) REFERENCES Employee(id) ON DELETE CASCADE;
