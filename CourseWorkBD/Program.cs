using Npgsql;
using System.Text;

while (true)
{
    Console.Write("Логин: ");
    var login = Console.ReadLine();
    Console.Write("Пароль: ");
    var password = Console.ReadLine();

    //var connStr = $"Host=localhost;Port=5432;Database=restaurant_db;Username={login};Password={password}";
    var connStr =
        $"Host=localhost;Port=5432;Database=restaurant_db;Username={login};Password={password};Include Error Detail=true";

    await using var conn = new NpgsqlConnection(connStr);
    try
    {
        await conn.OpenAsync();
        const string sql = "SELECT login(@u, @p);";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", login);
        cmd.Parameters.AddWithValue("p", password);
        var professionObj = await cmd.ExecuteScalarAsync();

        if (professionObj is null)
        {
            Console.WriteLine("Пользователь не найден в таблице Account.");
            continue;
        }

        var profession = (string)professionObj;
        Console.WriteLine($"\nРоль: {profession}");

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
                Console.WriteLine("Неизвестная профессия.");
                break;
        }
    }
    catch (PostgresException ex) when (ex.SqlState == "28P01")
    {
        Console.WriteLine("Пользователь не найден в таблице Account.");
    }
    catch (PostgresException ex)
    {
        Console.WriteLine($"Postgres ошибка: {ex.SqlState} — {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка подключения: {ex.Message}");
    }
}


static void AdminMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""

                          === Меню администратора ===

                          === Сотрудники ===
                          1. Просмотр информации о всех сотрудниках
                          2. Добавить нового сотрудника
                          3. Редактировать информацию о сотруднике по ID
                          4. Удалить сотрудника по ID

                          === Заказы ===
                          5. Просмотр информации о заказах

                          === Столики ===
                          6. Просмотр всех столиков
                          7. Изменить информацию о столике по ID

                          === Аккаунты ===
                          8. Просмотр всех аккаунтов
                          9. Добавить новый аккаунт
                          10. Редактировать информацию об аккаунте
                          11. Удалить аккаунт

                          0. Выход
                          """);

        Console.Write("Введите пункт меню: ");
        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_employees()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 40, 20, 10], "ID", "ФИО", "Профессия", "Зарплата");
                break;

            case "2":
                // ФИО
                string? name;
                while (true)
                {
                    Console.Write("ФИО (минимум 3 слова): ");
                    name = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(name) &&
                        name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3)
                        break;
                    Console.WriteLine("Ошибка: ФИО должно содержать как минимум 3 слова.");
                }

                // Профессия
                var validProfessions = new[] { "Официант", "Повар", "Администратор", "Кладовщик" };
                string? prof;
                while (true)
                {
                    Console.Write("Профессия (Администратор, Повар, Официант, Кладовщик): ");
                    prof = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(prof) && validProfessions.Contains(prof))
                        break;
                    Console.WriteLine("Ошибка: профессия должна быть одной из перечисленных.");
                }

                // Зарплата
                float sal;
                while (true)
                {
                    Console.Write("Зарплата (не менее 22440): ");
                    string? salaryInputS = Console.ReadLine();
                    if (float.TryParse(salaryInputS, out sal) && sal >= 22440)
                        break;
                    Console.WriteLine("Ошибка: зарплата должна быть числом не меньше 22440.");
                }

                // Вставка
                using (var cmd = new NpgsqlCommand("SELECT create_employee(@n, @p, @s)", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", prof);
                    cmd.Parameters.AddWithValue("s", sal);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Сотрудник добавлен.");
                }

                break;


            case "3":
                Console.Write("ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id))
                {
                    Console.WriteLine("Ошибка: некорректный ID.");
                    break;
                }

                // Получаем текущие данные сотрудника
                string currentName = "";
                float currentSalary = 0;

                using (var getCmd = new NpgsqlCommand("SELECT e.name, e.salary FROM employee e WHERE id = @i", conn))
                {
                    getCmd.Parameters.AddWithValue("i", id);
                    using var reader = getCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        currentName = reader.GetString(0);
                        currentSalary = reader.GetFloat(1);
                    }
                    else
                    {
                        Console.WriteLine("Сотрудник с таким ID не найден.");
                        break;
                    }
                }

                // ФИО
                string? newName;
                while (true)
                {
                    Console.Write($"Новое ФИО (текущее: {currentName}): ");
                    newName = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        newName = currentName;
                        break;
                    }

                    if (newName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3)
                        break;
                    Console.WriteLine("Ошибка: ФИО должно содержать минимум 3 слова.");
                }

                // Зарплата 
                float newSalary;
                while (true)
                {
                    Console.Write($"Новая зарплата (текущая: {currentSalary}): ");
                    string? salaryInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(salaryInput))
                    {
                        newSalary = currentSalary;
                        break;
                    }

                    if (float.TryParse(salaryInput, out newSalary) && newSalary >= 22440)
                        break;
                    Console.WriteLine("Ошибка: зарплата должна быть числом не меньше 22440.");
                }

                // Обновление
                using (var updateCmd = new NpgsqlCommand("SELECT update_employee(@i, @n, @s)", conn))
                {
                    updateCmd.Parameters.AddWithValue("i", id);
                    updateCmd.Parameters.AddWithValue("n", newName);
                    updateCmd.Parameters.AddWithValue("s", newSalary);
                    updateCmd.ExecuteNonQuery();
                    Console.WriteLine("Информация обновлена.");
                }

                break;


            case "4":
                int delIdEmp;
                while (true)
                {
                    Console.Write("ID для удаления: ");
                    if (!int.TryParse(Console.ReadLine(), out delIdEmp))
                    {
                        Console.WriteLine("Ошибка: некорректный ID.");
                        continue;
                    }

                    // Проверка существования сотрудника
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM employee WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", delIdEmp);
                        var count = (long)checkCmd.ExecuteScalar()!;
                        if (count == 0)
                        {
                            Console.WriteLine("Ошибка: сотрудник с таким ID не найден.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT delete_employee(@id)", conn))
                {
                    cmd.Parameters.AddWithValue("id", delIdEmp);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Сотрудник удалён.");
                }

                break;

            case "5":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_orders()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 10, 30, 10], "ID", "ID Стола", "Дата", "Статус");
                break;

            case "6":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_tables()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 20, 20], "ID", "Расположение", "Статус");
                break;

            case "7":
                int tableId;
                while (true)
                {
                    Console.Write("ID столика: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out tableId))
                    {
                        Console.WriteLine("Ошибка: введите целое число.");
                        continue;
                    }

                    // Проверка существования столика
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM tables WHERE id = @i", conn))
                    {
                        checkCmd.Parameters.AddWithValue("i", tableId);
                        var count = (long)checkCmd.ExecuteScalar()!;
                        if (count == 0)
                        {
                            Console.WriteLine("Ошибка: столик с таким ID не найден. Попробуйте снова.");
                            continue;
                        }
                    }

                    break;
                }

                // Получаем текущий статус
                string currentStatus = "";
                using (var getCmd = new NpgsqlCommand("SELECT t.status FROM tables t WHERE id = @i", conn))
                {
                    getCmd.Parameters.AddWithValue("i", tableId);
                    using var reader = getCmd.ExecuteReader();
                    if (reader.Read())
                        currentStatus = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                // Ввод нового статуса с проверкой допустимых значений
                string? newStatus;
                while (true)
                {
                    Console.Write($"Новый статус (текущий: {currentStatus}) (Занят, Свободен): ");
                    newStatus = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newStatus))
                    {
                        newStatus = currentStatus;
                        break;
                    }

                    newStatus = newStatus.Trim();
                    if (newStatus.Equals("Занят", StringComparison.OrdinalIgnoreCase) ||
                        newStatus.Equals("Свободен", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    Console.WriteLine("Ошибка: статус должен быть 'Занят' или 'Свободен'.");
                }

                // Обновляем
                using (var updateCmd = new NpgsqlCommand("SELECT update_table_status(@i, @s)", conn))
                {
                    updateCmd.Parameters.AddWithValue("i", tableId);
                    updateCmd.Parameters.AddWithValue("s", newStatus);
                    updateCmd.ExecuteNonQuery();
                    Console.WriteLine("Статус обновлён.");
                }

                break;


            case "8":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_accounts()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 20, 10], "ID", "Логин", "ID Сотрудника");
                break;


            case "9":
                string? newUsername;
                while (true)
                {
                    Console.Write("Введите имя пользователя: ");
                    newUsername = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newUsername))
                    {
                        Console.WriteLine("Логин не может быть пустым.");
                        continue;
                    }
                    break;
                }

                string? newPassword;
                while (true)
                {
                    Console.Write("Введите пароль: ");
                    newPassword = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(newPassword))
                    {
                        Console.WriteLine("Пароль не может быть пустым.");
                        continue;
                    }
                    break;
                }

                int newEmpId;
                while (true)
                {
                    Console.Write("Введите ID сотрудника: ");
                    var empIdInputS = Console.ReadLine();
                    if (!int.TryParse(empIdInputS, out newEmpId))
                    {
                        Console.WriteLine("Ошибка: введите корректное число.");
                        continue;
                    }

                    // Проверка существования сотрудника
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM employee WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", newEmpId);
                        var count = (long)checkCmd.ExecuteScalar()!;
                        if (count == 0)
                        {
                            Console.WriteLine("Ошибка: сотрудник с таким ID не найден.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT create_account(@u, @p, @e)", conn))
                {
                    cmd.Parameters.AddWithValue("u", newUsername);
                    cmd.Parameters.AddWithValue("p", newPassword);
                    cmd.Parameters.AddWithValue("e", newEmpId);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Аккаунт создан.");
                }

                break;


            case "10":
                string currentUsername = "";
                string currentPassword = "";
                int currentEmployeeId = 0;
                int accId;
                while (true)
                {
                    Console.Write("ID аккаунта: ");
                    if (!int.TryParse(Console.ReadLine(), out accId))
                    {
                        Console.WriteLine("Некорректный ID.");
                       continue;
                    }
                    using (var getCmd =
                           new NpgsqlCommand("SELECT username, password, employee_id FROM account WHERE id = @i", conn))
                    {
                        getCmd.Parameters.AddWithValue("i", accId);
                        using var reader = getCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            currentUsername = reader.GetString(0);
                            currentPassword = reader.GetString(1);
                            currentEmployeeId = reader.GetInt32(2);
                        }
                        else
                        {
                            Console.WriteLine("Аккаунт с таким ID не найден.");
                            continue;
                        }
                    }

                    break;
                }
                
                Console.Write($"Новое имя пользователя (текущее: {currentUsername}): ");
                string? newUsernameCh = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newUsernameCh))
                    newUsernameCh = currentUsername;

                Console.Write($"Новый пароль (текущий: {currentPassword}): ");
                string? newPasswordCh = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(newPasswordCh))
                    newPasswordCh = currentPassword;

                Console.Write($"Новый ID сотрудника (текущий: {currentEmployeeId}): ");
                string? empIdInput = Console.ReadLine();
                int newEmpIdCh = string.IsNullOrWhiteSpace(empIdInput) ? currentEmployeeId : int.Parse(empIdInput);
                
                using (var updateCmd = new NpgsqlCommand("SELECT update_account(@i, @u, @p, @e)", conn))
                {
                    updateCmd.Parameters.AddWithValue("i", accId);
                    updateCmd.Parameters.AddWithValue("u", newUsernameCh);
                    updateCmd.Parameters.AddWithValue("p", newPasswordCh);
                    updateCmd.Parameters.AddWithValue("e", newEmpIdCh);
                    updateCmd.ExecuteNonQuery();
                    Console.WriteLine("Аккаунт обновлён.");
                }

                break;


            case "11":
                int delId;
                while (true)
                {
                    Console.Write("Введите ID аккаунта для удаления: ");
                    var input = Console.ReadLine();
                    if (!int.TryParse(input, out delId))
                    {
                        Console.WriteLine("Некорректный формат ID.");
                        continue;
                    }
                    
                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM account WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", delId);
                        var count = (long)checkCmd.ExecuteScalar()!;
                        if (count == 0)
                        {
                            Console.WriteLine("Аккаунт с таким ID не найден.");
                            continue;
                        }
                    }
                    
                    using (var cmd = new NpgsqlCommand("SELECT delete_account(@id)", conn))
                    {
                        cmd.Parameters.AddWithValue("id", delId);
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Аккаунт удалён.");
                    }

                    break;
                }

                break;


            case "0":
                return;

            default:
                Console.WriteLine("Неверный пункт меню.");
                break;
        }
    }
}

static void PrintTable(NpgsqlDataReader reader, int[] columnWidths, params string[] headers)
{
    for (int i = 0; i < headers.Length; i++)
        Console.Write(headers[i].PadRight(columnWidths[i]));
    Console.WriteLine();
    
    for (int i = 0; i < columnWidths.Length; i++)
        Console.Write(new string('-', columnWidths[i]));
    Console.WriteLine();


    while (reader.Read())
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
            Console.Write(value!.PadRight(columnWidths[i]));
        }

        Console.WriteLine();
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
            pwd.Length--;
            Console.Write("\b \b");
        }
        else if (!char.IsControl(k.KeyChar))
        {
            pwd.Append(k.KeyChar);
            Console.Write('*');
        }
    }

    Console.WriteLine();
    return pwd.ToString();
}