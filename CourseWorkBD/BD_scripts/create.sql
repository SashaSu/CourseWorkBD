-- Таблица сотрудников
CREATE TABLE IF NOT EXISTS Employee
(
    id         SERIAL PRIMARY KEY,
    name       VARCHAR(256) NOT NULL,
    profession VARCHAR(256) NOT NULL,
    salary      FLOAT NOT NULL 
);

-- Таблица заказов
CREATE TABLE IF NOT EXISTS Orders
(
    id       SERIAL PRIMARY KEY,
    table_id INT       NOT NULL,
    time_order     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status   VARCHAR(256)      NOT NULL
);

-- Таблица столов
CREATE TABLE IF NOT EXISTS Tables
(
    id       SERIAL PRIMARY KEY,
    location VARCHAR(256) NOT NULL,
    status   VARCHAR(256) NOT NULL 
);

-- Таблица блюд
CREATE TABLE IF NOT EXISTS Dish
(
    id       SERIAL PRIMARY KEY,
    dish_name     VARCHAR(256) NOT NULL,
    price FLOAT,
    calories INT  NOT NULL,
    weight   INT  NOT NULL
);

-- Таблица продуктов
CREATE TABLE IF NOT EXISTS Product
(
    id   SERIAL PRIMARY KEY,
    product_name VARCHAR(256) NOT NULL,
    product_type VARCHAR(256) NOT NULL
);

-- Таблица складов
CREATE TABLE IF NOT EXISTS Warehouse
(
    id   SERIAL PRIMARY KEY,
    warehouse_type VARCHAR(256) NOT NULL
);

-- Таблица хранимых продуктов
CREATE TABLE IF NOT EXISTS Stored_product
(
    id           SERIAL PRIMARY KEY,
    warehouse_id INT  NOT NULL,
    product_id   INT  NOT NULL,
    quantity     FLOAT NOT NULL 
);

-- Ассоциативная таблица сотрудник-заказ
CREATE TABLE IF NOT EXISTS Employee_Order
(
    employee_id INT NOT NULL,
    order_id    INT NOT NULL
    );

-- Ассоциативная таблица заказ-блюдо
CREATE TABLE IF NOT EXISTS Order_Dish
(
    order_id INT NOT NULL,
    dish_id  INT NOT NULL
    );

-- Ассоциативная таблица блюдо-продукт
CREATE TABLE IF NOT EXISTS Dish_Product
(
    dish_id    INT NOT NULL,
    product_id INT NOT NULL,
    quantity   FLOAT NOT NULL
);

-- Аккаунт
CREATE TABLE IF NOT EXISTS Account
(
    id           SERIAL PRIMARY KEY,
    employee_id  INT NOT NULL UNIQUE,
    username     VARCHAR(256) NOT NULL UNIQUE,
    password     VARCHAR(256) NOT NULL
);

