using Npgsql;
using System.Text;

while (true)
{
    Console.Write("Логин: ");
    var login    = Console.ReadLine();
    Console.Write("Пароль: ");
    var password = Console.ReadLine();

    var connStr  = $"Host=localhost;Port=5432;Database=restaurant_db;Username={login};Password={password}";
    await using var conn = new NpgsqlConnection(connStr);
    try
    {
        await conn.OpenAsync();
        const string sql = """
            SELECT e.profession
            FROM   Account a
            JOIN   Employee e ON e.id = a.employee_id
            WHERE  a.username = @u
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", login);
        var professionObj = await cmd.ExecuteScalarAsync();

        if (professionObj is null)
        {
            Console.WriteLine("Пользователь не найден в таблице Account.");
            continue;
        }

        var profession = (string)professionObj;
        Console.WriteLine($"\nЗдравствуйте, {profession}!");

        switch (profession)
        {
            case "Администратор":
                AdminMenu(conn);
                break;
            case "Официант":
                WaiterMenu(conn);
                break;
            case "Повар":
                CookMenu(conn);
                break;
            case "Кладовщик":
                WarehouseMenu(conn);
                break;
            default:
                Console.WriteLine("Неизвестная профессия."); break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nОшибка подключения: {ex.Message}\n");
    }
}


static void AdminMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""
            === Меню администратора ===
            1. Просмотр сотрудников
            2. Добавить сотрудника
            3. Редактировать сотрудника
            4. Удалить сотрудника
            5. Просмотр заказов
            0. Выход
            """);
        var key = Console.ReadLine();
        switch (key)
        {
            // case "1": EmployeeDal.PrintAll(conn); break;
            // case "2": EmployeeDal.Create(conn);   break;
            // case "3": EmployeeDal.Update(conn);   break;
            // case "4": EmployeeDal.Delete(conn);   break;
            // case "5": OrderDal.PrintAll(conn);    break;
            // case "0": return;
        }
    }
}

static void WaiterMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""
            === Меню официанта ===
            1. Просмотр столов
            2. Создать заказ
            3. Добавить блюдо в заказ
            0. Назад
            """);
        var key = Console.ReadLine();
        switch (key)
        {
            // case "1": TableDal.PrintAll(conn);      break;
            // case "2": OrderDal.Create(conn);        break;
            // case "3": OrderDishDal.AddDish(conn);   break;
            // case "0": return;
        }
    }
}

static void CookMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""
            === Меню повара ===
            1. Просмотр заказов
            2. Изменить статус заказа
            0. Назад
            """);
        var key = Console.ReadLine();
        switch (key)
        {
            // case "1": OrderDal.PrintAll(conn);      break;
            // case "2": OrderDal.UpdateStatus(conn);  break;
            // case "0": return;
        }
    }
}

static void WarehouseMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""
            === Меню кладовщика ===
            1. Просмотр продуктов на складе
            2. Приход/списание продукта
            0. Назад
            """);
        var key = Console.ReadLine();
        switch (key)
        {
            // case "1": StoredProductDal.PrintAll(conn);  break;
            // case "2": StoredProductDal.UpdateQty(conn); break;
            // case "0": return;
        }
    }
}

// ---------- Вспомогательные методы ----------

static string ReadPassword()
{
    var pwd = new StringBuilder();
    ConsoleKeyInfo k;
    while ((k = Console.ReadKey(true)).Key != ConsoleKey.Enter)
    {
        if (k.Key == ConsoleKey.Backspace && pwd.Length > 0)
        {
            pwd.Length--; Console.Write("\b \b");
        }
        else if (!char.IsControl(k.KeyChar))
        {
            pwd.Append(k.KeyChar); Console.Write('*');
        }
    }
    Console.WriteLine();
    return pwd.ToString();
}
