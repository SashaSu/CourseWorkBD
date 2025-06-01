REVOKE ALL ON account FROM PUBLIC;

GRANT EXECUTE ON FUNCTION login(VARCHAR(256), VARCHAR(256)) TO PUBLIC;

CREATE ROLE waiter_role LOGIN PASSWORD 'waiter';

CREATE ROLE cook_role LOGIN PASSWORD 'cook';

CREATE ROLE admin_role LOGIN PASSWORD 'admin';

CREATE ROLE warehouse_manager_role LOGIN PASSWORD 'manager';

-- Официант
GRANT SELECT, INSERT, UPDATE, DELETE ON Orders TO waiter_role;
GRANT SELECT ON Tables TO waiter_role;
GRANT SELECT ON Dish TO waiter_role; 
GRANT SELECT, INSERT, UPDATE, DELETE ON order_dish TO waiter_role;
GRANT SELECT ON dish_product TO waiter_role;
GRANT ALL PRIVILEGES ON stored_product TO waiter_role;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE orders_id_seq TO waiter_role;

-- Повар
GRANT SELECT, UPDATE ON Orders TO cook_role; 
GRANT SELECT, UPDATE ON Order_Dish TO cook_role; 
GRANT SELECT ON Product TO cook_role;
GRANT SELECT ON Dish_Product TO cook_role;
GRANT SELECT ON Dish TO cook_role;

-- Кладовщик
GRANT SELECT, INSERT, UPDATE, DELETE ON Product TO warehouse_manager_role;
GRANT SELECT, INSERT, UPDATE ON Warehouse TO warehouse_manager_role; 
GRANT SELECT, INSERT, UPDATE, DELETE ON Stored_product TO warehouse_manager_role; 
GRANT USAGE, SELECT, UPDATE ON SEQUENCE stored_product_id_seq TO warehouse_manager_role;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE product_id_seq TO warehouse_manager_role;

-- Админ
                                                                          
REVOKE ALL PRIVILEGES ON ALL TABLES IN SCHEMA public FROM admin_role;                                                                        
GRANT SELECT, INSERT, UPDATE, DELETE ON Employee TO admin_role;
GRANT SELECT ON Orders TO admin_role;
GRANT SELECT, UPDATE ON Tables TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Account TO admin_role;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE employee_id_seq TO admin_role;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE account_id_seq TO admin_role;

-- Регистрация

CREATE USER admin1 WITH PASSWORD 'admin'; GRANT admin_role TO admin1;
CREATE USER admin2 WITH PASSWORD 'admin'; GRANT admin_role TO admin2;
CREATE USER manager3 WITH PASSWORD 'manager'; GRANT warehouse_manager_role TO manager3;
CREATE USER cook4 WITH PASSWORD 'cook'; GRANT cook_role TO cook4;
CREATE USER admin5 WITH PASSWORD 'admin'; GRANT admin_role TO admin5;
CREATE USER cook6 WITH PASSWORD 'cook'; GRANT cook_role TO cook6;
CREATE USER waiter7 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter7;
CREATE USER cook8 WITH PASSWORD 'cook'; GRANT cook_role TO cook8;
CREATE USER manager9 WITH PASSWORD 'manager'; GRANT warehouse_manager_role TO manager9;
CREATE USER manager10 WITH PASSWORD 'manager'; GRANT warehouse_manager_role TO manager10;
CREATE USER cook11 WITH PASSWORD 'cook'; GRANT cook_role TO cook11;
CREATE USER cook12 WITH PASSWORD 'cook'; GRANT cook_role TO cook12;
CREATE USER waiter13 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter13;
CREATE USER admin14 WITH PASSWORD 'admin'; GRANT admin_role TO admin14;
CREATE USER manager15 WITH PASSWORD 'manager'; GRANT warehouse_manager_role TO manager15;
CREATE USER admin16 WITH PASSWORD 'admin'; GRANT admin_role TO admin16;
CREATE USER cook17 WITH PASSWORD 'cook'; GRANT cook_role TO cook17;
CREATE USER admin18 WITH PASSWORD 'admin'; GRANT admin_role TO admin18;
CREATE USER admin19 WITH PASSWORD 'admin'; GRANT admin_role TO admin19;
CREATE USER waiter20 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter20;
CREATE USER waiter21 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter21;
CREATE USER admin22 WITH PASSWORD 'admin'; GRANT admin_role TO admin22;
CREATE USER admin23 WITH PASSWORD 'admin'; GRANT admin_role TO admin23;
CREATE USER cook24 WITH PASSWORD 'cook'; GRANT cook_role TO cook24;
CREATE USER admin25 WITH PASSWORD 'admin'; GRANT admin_role TO admin25;
CREATE USER waiter26 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter26;
CREATE USER waiter27 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter27;
CREATE USER waiter28 WITH PASSWORD 'waiter'; GRANT waiter_role TO waiter28;
CREATE USER manager29 WITH PASSWORD 'manager'; GRANT warehouse_manager_role TO manager29;
CREATE USER cook30 WITH PASSWORD 'cook'; GRANT cook_role TO cook30;
