using Npgsql;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace RestaurantTests
{
    public class OrderDishTriggerTests
    {
        private const string ConnectionString = "Host=localhost;Username=postgres;Password=123;Database=restaurant_db";

        private int _testDishId;
        private int _testProductId;
        private int _orderId;
        private int _warehouseId;
        private int _tableId;

        [SetUp]
        public async Task Setup()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();
            var resetSequences = new NpgsqlCommand(@"
DO $$
DECLARE
    max_id int;
BEGIN
    SELECT COALESCE(MAX(id), 0) + 1 INTO max_id FROM Dish;
    EXECUTE format('ALTER SEQUENCE dish_id_seq RESTART WITH %s', max_id);

    SELECT COALESCE(MAX(id), 0) + 1 INTO max_id FROM Product;
    EXECUTE format('ALTER SEQUENCE product_id_seq RESTART WITH %s', max_id);

    SELECT COALESCE(MAX(id), 0) + 1 INTO max_id FROM Warehouse;
    EXECUTE format('ALTER SEQUENCE warehouse_id_seq RESTART WITH %s', max_id);

    SELECT COALESCE(MAX(id), 0) + 1 INTO max_id FROM Tables;
    EXECUTE format('ALTER SEQUENCE tables_id_seq RESTART WITH %s', max_id);

    SELECT COALESCE(MAX(id), 0) + 1 INTO max_id FROM Orders;
    EXECUTE format('ALTER SEQUENCE orders_id_seq RESTART WITH %s', max_id);
END $$;
", connection);
            await resetSequences.ExecuteNonQueryAsync();
            
            var insertProduct = new NpgsqlCommand(
                "INSERT INTO Product(product_name, product_type) VALUES ('TestProduct', 'Овощ') RETURNING id",
                connection);
            _testProductId = Convert.ToInt32(await insertProduct.ExecuteScalarAsync());
            
            var insertDish = new NpgsqlCommand(
                "INSERT INTO Dish(dish_name, calories, weight) VALUES ('TestDish', 500, 300) RETURNING id", connection);
            _testDishId = Convert.ToInt32(await insertDish.ExecuteScalarAsync());
            
            var insertDishProduct = new NpgsqlCommand(
                "INSERT INTO Dish_Product(dish_id, product_id, quantity) VALUES (@dish, @prod, 5)", connection);
            insertDishProduct.Parameters.AddWithValue("dish", _testDishId);
            insertDishProduct.Parameters.AddWithValue("prod", _testProductId);
            await insertDishProduct.ExecuteNonQueryAsync();
            
            var insertWarehouse = new NpgsqlCommand(
                "INSERT INTO Warehouse(warehouse_type) VALUES ('Обычный') RETURNING id", connection);
            _warehouseId = Convert.ToInt32(await insertWarehouse.ExecuteScalarAsync());
            
            var insertStored = new NpgsqlCommand(
                "INSERT INTO Stored_Product(warehouse_id, product_id, quantity) VALUES (@wh, @prod, 10)", connection);
            insertStored.Parameters.AddWithValue("wh", _warehouseId);
            insertStored.Parameters.AddWithValue("prod", _testProductId);
            await insertStored.ExecuteNonQueryAsync();
            
            var insertTable = new NpgsqlCommand(
                "INSERT INTO Tables(location, status) VALUES ('Зал', 'Свободен') RETURNING id", connection);
            _tableId = Convert.ToInt32(await insertTable.ExecuteScalarAsync());
            
            var insertOrder = new NpgsqlCommand(
                "INSERT INTO Orders(table_id, status) VALUES (@table, 'Принят') RETURNING id", connection);
            insertOrder.Parameters.AddWithValue("table", _tableId);
            _orderId = Convert.ToInt32(await insertOrder.ExecuteScalarAsync());
        }

        [Test]
        public async Task AddDishToOrder_ShouldSubtractProductQuantity()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var insert = new NpgsqlCommand(
                "INSERT INTO Order_Dish(order_id, dish_id) VALUES (@order, @dish)", connection);
            insert.Parameters.AddWithValue("order", _orderId);
            insert.Parameters.AddWithValue("dish", _testDishId);
            await insert.ExecuteNonQueryAsync();

            var checkQuantity = new NpgsqlCommand(
                "SELECT s.quantity FROM Stored_Product s WHERE product_id = @prod", connection);
            checkQuantity.Parameters.AddWithValue("prod", _testProductId);
            var qty = Convert.ToDouble(await checkQuantity.ExecuteScalarAsync());

            Assert.AreEqual(5.0, qty, "Ожидалось уменьшение количества продукта до 5.");
        }

        [Test]
        public async Task AddDishToOrder_ShouldThrow_WhenNotEnoughProduct()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.OpenAsync();

            var update = new NpgsqlCommand(
                "UPDATE Stored_Product SET quantity = 1 WHERE product_id = @prod", connection);
            update.Parameters.AddWithValue("prod", _testProductId);
            await update.ExecuteNonQueryAsync();

            var insert = new NpgsqlCommand(
                "INSERT INTO Order_Dish(order_id, dish_id) VALUES (@order, @dish)", connection);
            insert.Parameters.AddWithValue("order", _orderId);
            insert.Parameters.AddWithValue("dish", _testDishId);

            var ex = Assert.ThrowsAsync<PostgresException>(async () => await insert.ExecuteNonQueryAsync());
            StringAssert.Contains("Недостаточно продукта", ex.MessageText);
        }
    }
}