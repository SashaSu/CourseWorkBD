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

                          === Столики ===
                          1. Просмотр всех столиков

                          === Блюда ===
                          2. Просмотр всех блюд

                          === Заказы ===
                          3. Просмотр всех заказов
                          4. Добавить новый заказ
                          5. Просмотреть блюда в заказе
                          6. Добавить блюдо в заказ
                          7. Изменить блюда в заказе
                          8. Удалить блюдо из заказа
                          9. Изменить статус заказа
                          10. Удалить заказ

                          0. Выход
                          """);
        Console.Write("Введите пункт меню: ");
        var key = Console.ReadLine();
        switch (key)
        {
            case "1":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_tables()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 20, 20], "ID", "Расположение", "Статус");
                break;
            case "2":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_dishes()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 70, 20, 30, 20], "ID", "Название", "Цена", "Калориийность", "Вес");
                break;
            case "3":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_orders()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 10, 30, 10], "ID", "ID Стола", "Дата", "Статус");
                break;
            case "4":
                int tableId;
                while (true)
                {
                    Console.Write("Введите ID столика: ");
                    if (!int.TryParse(Console.ReadLine(), out tableId))
                    {
                        Console.WriteLine("Некорректный ID столика.");
                        continue;
                    }

                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Tables WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", tableId);
                        if ((long)checkCmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Столик с таким ID не найден.");
                            continue;
                        }
                    }

                    break;
                }

                string[] allowedStatuses = { "Принят", "Готовится", "Готов", "Выдан", "Оплачен" };
                string? status;
                status = "Принят";

                using (var cmd = new NpgsqlCommand("SELECT create_order(@t, @s)", conn))
                {
                    cmd.Parameters.AddWithValue("t", tableId);
                    cmd.Parameters.AddWithValue("s", status);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Заказ добавлен.");
                }

                break;
            case "5":
                int orderIdToCheck;
                while (true)
                {
                    Console.Write("Введите ID заказа для просмотра блюд: ");
                    if (!int.TryParse(Console.ReadLine(), out orderIdToCheck))
                    {
                        Console.WriteLine("Некорректный ID. Попробуйте ещё раз.");
                        continue;
                    }

                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", orderIdToCheck);
                        var count = (long)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            Console.WriteLine("Заказ с таким ID не найден. Попробуйте ещё раз.");
                            continue;
                        }

                        break;
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT * FROM get_dishes_in_order(@id)", conn))
                {
                    cmd.Parameters.AddWithValue("id", orderIdToCheck);
                    using var reader = cmd.ExecuteReader();
                    PrintTable(reader, new[] { 10, 10, 70 }, "ID заказа", "ID блюда", "Название блюда");
                }

                break;


            case "6":
                int orderId, dishId;

                while (true)
                {
                    Console.Write("Введите ID заказа: ");
                    if (!int.TryParse(Console.ReadLine(), out orderId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var checkOrderCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        checkOrderCmd.Parameters.AddWithValue("id", orderId);
                        if ((long)checkOrderCmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Заказ с таким ID не найден.");
                            continue;
                        }
                    }

                    break;
                }

                while (true)
                {
                    Console.Write("Введите ID блюда: ");
                    if (!int.TryParse(Console.ReadLine(), out dishId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var checkDishCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Dish WHERE id = @id", conn))
                    {
                        checkDishCmd.Parameters.AddWithValue("id", dishId);
                        if ((long)checkDishCmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Блюдо с таким ID не найдено.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT add_dish_to_order(@o, @d)", conn))
                {
                    cmd.Parameters.AddWithValue("o", orderId);
                    cmd.Parameters.AddWithValue("d", dishId);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Блюдо добавлено в заказ.");
                }

                break;

            case "7":
                int updOrderId6, oldDishId6, newDishId6;

                while (true)
                {
                    Console.Write("Введите ID заказа: ");
                    if (!int.TryParse(Console.ReadLine(), out updOrderId6))
                    {
                        Console.WriteLine("Некорректный ввод. Попробуйте снова.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", updOrderId6);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Заказ с таким ID не найден.");
                            continue;
                        }
                    }

                    break;
                }

                while (true)
                {
                    Console.Write("Введите ID старого блюда: ");
                    if (!int.TryParse(Console.ReadLine(), out oldDishId6))
                    {
                        Console.WriteLine("Некорректный ID. Попробуйте снова.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Dish WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", oldDishId6);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Блюдо не найдено.");
                            continue;
                        }
                    }

                    break;
                }

                while (true)
                {
                    Console.Write("Введите ID нового блюда: ");
                    if (!int.TryParse(Console.ReadLine(), out newDishId6))
                    {
                        Console.WriteLine("Некорректный ID. Попробуйте снова.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Dish WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", newDishId6);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Блюдо не найдено.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT update_dish_in_order(@order_id, @old_dish, @new_dish)",
                           conn))
                {
                    cmd.Parameters.AddWithValue("order_id", updOrderId6);
                    cmd.Parameters.AddWithValue("old_dish", oldDishId6);
                    cmd.Parameters.AddWithValue("new_dish", newDishId6);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Блюдо в заказе успешно обновлено.");
                    }
                    catch (PostgresException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.MessageText}");
                    }
                }

                break;
            case "8":
                int delOrderId7, delDishId7;

                while (true)
                {
                    Console.Write("Введите ID заказа: ");
                    if (!int.TryParse(Console.ReadLine(), out delOrderId7))
                    {
                        Console.WriteLine("Некорректный ввод.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", delOrderId7);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Заказ не найден.");
                            continue;
                        }
                    }

                    break;
                }

                while (true)
                {
                    Console.Write("Введите ID блюда для удаления: ");
                    if (!int.TryParse(Console.ReadLine(), out delDishId7))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Dish WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", delDishId7);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Блюдо не найдено.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT delete_dish_from_order(@order_id, @dish_id)", conn))
                {
                    cmd.Parameters.AddWithValue("order_id", delOrderId7);
                    cmd.Parameters.AddWithValue("dish_id", delDishId7);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Блюдо удалено из заказа.");
                    }
                    catch (PostgresException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.MessageText}");
                    }
                }

                break;
            case "9":
                int updOrderId8;

                while (true)
                {
                    Console.Write("Введите ID заказа для изменения статуса: ");
                    if (!int.TryParse(Console.ReadLine(), out updOrderId8))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", updOrderId8);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Заказ не найден.");
                            continue;
                        }
                    }

                    break;
                }

                string? newStatus;
                while (true)
                {
                    Console.Write("Введите новый статус заказа (Принят, Выдан, Оплачен): ");
                    newStatus = Console.ReadLine();

                    var validStatuses = new[] { "Принят", "Выдан", "Оплачен" };
                    if (validStatuses.Contains(newStatus))
                        break;

                    Console.WriteLine("Недопустимый статус. ");
                }

                using (var cmd = new NpgsqlCommand("SELECT update_order(@id, @status)", conn))
                {
                    cmd.Parameters.AddWithValue("id", updOrderId8);
                    cmd.Parameters.AddWithValue("status", newStatus);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Статус заказа успешно обновлён.");
                    }
                    catch (PostgresException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.MessageText}");
                    }
                }

                break;
            case "10":
                int delOrderId9;

                while (true)
                {
                    Console.Write("Введите ID заказа для удаления: ");
                    if (!int.TryParse(Console.ReadLine(), out delOrderId9))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", delOrderId9);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Заказ не найден.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT delete_order(@id)", conn))
                {
                    cmd.Parameters.AddWithValue("id", delOrderId9);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Заказ удалён.");
                    }
                    catch (PostgresException ex)
                    {
                        Console.WriteLine($"Ошибка при удалении: {ex.MessageText}");
                    }
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

static void CookMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""

                          === Меню повара ===

                          == Блюда ==
                          1. Просмотр информации о блюдах в заказе
                          2. Просмотр информации о продуктах в блюде

                          == Заказы ==
                          3. Просмотр заказов
                          4. Изменение статуса заказа

                          0. Выход
                          """);
        Console.Write("Введите пункт меню: ");
        var key = Console.ReadLine();
        switch (key)
        {
            case "1":
                int orderIdToCheck;
                while (true)
                {
                    Console.Write("Введите ID заказа для просмотра блюд: ");
                    if (!int.TryParse(Console.ReadLine(), out orderIdToCheck))
                    {
                        Console.WriteLine("Некорректный ID. Попробуйте ещё раз.");
                        continue;
                    }

                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", orderIdToCheck);
                        var count = (long)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            Console.WriteLine("Заказ с таким ID не найден. Попробуйте ещё раз.");
                            continue;
                        }

                        break;
                    }
                }

                using (var cmd = new NpgsqlCommand("SELECT * FROM get_dishes_in_order(@id)", conn))
                {
                    cmd.Parameters.AddWithValue("id", orderIdToCheck);
                    using var reader = cmd.ExecuteReader();
                    PrintTable(reader, new[] { 10, 10, 70 }, "ID заказа", "ID блюда", "Название блюда");
                }

                break;
            case "2":
                int dishIdToCheck;
                while (true)
                {
                    Console.Write("Введите ID блюда для просмотра продуктов: ");
                    if (!int.TryParse(Console.ReadLine(), out dishIdToCheck))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM Dish WHERE id = @id", conn))
                    {
                        checkCmd.Parameters.AddWithValue("id", dishIdToCheck);
                        var count = (long)checkCmd.ExecuteScalar();

                        if (count == 0)
                        {
                            Console.WriteLine("Блюдо с таким ID не найдено.");
                            continue;
                        }
                    }

                    break;
                }

                using (var cmd = new NpgsqlCommand("SELECT * FROM get_products_in_dish(@id)", conn))
                {
                    cmd.Parameters.AddWithValue("id", dishIdToCheck);
                    using var reader = cmd.ExecuteReader();
                    PrintTable(reader, [10, 70], "ID", "Название");
                }

                break;
            case "3":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_orders()", conn))
                using (var reader = cmd.ExecuteReader())
                    PrintTable(reader, [10, 10, 30, 10], "ID", "ID Стола", "Дата", "Статус");
                break;
            case "4":
                int updOrderId8;

                while (true)
                {
                    Console.Write("Введите ID заказа для изменения статуса: ");
                    if (!int.TryParse(Console.ReadLine(), out updOrderId8))
                    {
                        Console.WriteLine("Некорректный ID.");
                        continue;
                    }

                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", updOrderId8);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Заказ не найден.");
                            continue;
                        }
                    }

                    break;
                }

                string? newStatus;
                while (true)
                {
                    Console.Write("Введите новый статус заказа (Готовится, Готов): ");
                    newStatus = Console.ReadLine();

                    var validStatuses = new[] { "Готовится", "Готов" };
                    if (validStatuses.Contains(newStatus))
                        break;

                    Console.WriteLine("Недопустимый статус. ");
                }

                using (var cmd = new NpgsqlCommand("SELECT update_order(@id, @status)", conn))
                {
                    cmd.Parameters.AddWithValue("id", updOrderId8);
                    cmd.Parameters.AddWithValue("status", newStatus);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Статус заказа успешно обновлён.");
                    }
                    catch (PostgresException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.MessageText}");
                    }
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

static void WarehouseMenu(NpgsqlConnection conn)
{
    while (true)
    {
        Console.WriteLine("""

                          === Меню кладовщика ===

                          == Хранящиеся продукты ==

                          1. Просмотр хран. продуктов
                          2. Добавить хран. продукт
                          3. Изменить информацию о хран. продукте
                          4. Удалить хран. продукт
                          5. Просмотр информации о складе хран. продукта

                          == Продукты ==

                          6. Просмотр продуктов
                          7. Добавить продукт
                          8. Изменить информацию о продукте
                          9. Удалить продукт
                          0. Выход
                          """);
        Console.Write("Введите пункт меню: ");
        var key = Console.ReadLine();
        switch (key)
        {
            case "1":
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_stored_products()", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    PrintTable(reader, [10, 10, 15, 10], "ID", "ID Склада", "ID Продукта", "Кол-во");
                }

                break;
            
            case "2":
            {
                int warehouseId;
                while (true)
                {
                    Console.Write("ID склада: ");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out warehouseId))
                    {
                        using (var checkWarehouseCmd =
                               new NpgsqlCommand("SELECT COUNT(*) FROM warehouse WHERE id = @id", conn))
                        {
                            checkWarehouseCmd.Parameters.AddWithValue("id", warehouseId);
                            var count = (long)checkWarehouseCmd.ExecuteScalar();
                            if (count > 0)
                                break; // склад найден
                            else
                                Console.WriteLine("Склад с таким ID не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: некорректный ID склада. ");
                    }
                }

                int productIdAdd;
                while (true)
                {
                    Console.Write("ID продукта: ");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out productIdAdd))
                    {
                        using (var checkProductCmd =
                               new NpgsqlCommand("SELECT COUNT(*) FROM product WHERE id = @id", conn))
                        {
                            checkProductCmd.Parameters.AddWithValue("id", productIdAdd);
                            var count = (long)checkProductCmd.ExecuteScalar();
                            if (count > 0)
                                break; // продукт найден
                            else
                                Console.WriteLine("Продукт с таким ID не найден. ");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ошибка: некорректный ID продукта. ");
                    }
                }

                float quantity;
                while (true)
                {
                    Console.Write("Количество: ");
                    string? input = Console.ReadLine();
                    if (float.TryParse(input, out quantity) && quantity >= 0)
                        break;
                    Console.WriteLine("Количество должно быть неотрицательным числом.");
                }

                using (var cmd = new NpgsqlCommand("SELECT add_stored_product(@wid, @pid, @q)", conn))
                {
                    cmd.Parameters.AddWithValue("wid", warehouseId);
                    cmd.Parameters.AddWithValue("pid", productIdAdd);
                    cmd.Parameters.AddWithValue("q", quantity);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Хранимый продукт добавлен.");
                }
            }
                break;
            case "3":
            {
                int storedId;
                while (true)
                {
                    Console.Write("ID хранимого продукта: ");
                    if (int.TryParse(Console.ReadLine(), out storedId))
                        break;
                    Console.WriteLine("Некорректный ID.");
                }

                int currentWarehouseId = 0;
                int currentProductId = 0;
                float currentQuantity = 0;

                using (var getCmd =
                       new NpgsqlCommand("SELECT warehouse_id, product_id, quantity FROM stored_product WHERE id = @i",
                           conn))
                {
                    getCmd.Parameters.AddWithValue("i", storedId);
                    using var reader = getCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        currentWarehouseId = reader.GetInt32(0);
                        currentProductId = reader.GetInt32(1);
                        currentQuantity = reader.GetFloat(2);
                    }
                    else
                    {
                        Console.WriteLine("Хранимый продукт с таким ID не найден.");
                        break;
                    }
                }

                int newWarehouseId;
                while (true)
                {
                    Console.Write($"ID склада (текущий: {currentWarehouseId}): ");
                    string? warehouseInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(warehouseInput))
                    {
                        newWarehouseId = currentWarehouseId;
                        break;
                    }

                    if (int.TryParse(warehouseInput, out newWarehouseId))
                    {
                        using (var checkWarehouseCmd =
                               new NpgsqlCommand("SELECT COUNT(*) FROM warehouse WHERE id = @id", conn))
                        {
                            checkWarehouseCmd.Parameters.AddWithValue("id", newWarehouseId);
                            var count = (long)checkWarehouseCmd.ExecuteScalar();
                            if (count > 0) break;
                            else Console.WriteLine("Склад с таким ID не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ID склада.");
                    }
                }

                int newProductId;
                while (true)
                {
                    Console.Write($"ID продукта (текущий: {currentProductId}): ");
                    string? productInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(productInput))
                    {
                        newProductId = currentProductId;
                        break;
                    }

                    if (int.TryParse(productInput, out newProductId))
                    {
                        using (var checkProductCmd =
                               new NpgsqlCommand("SELECT COUNT(*) FROM product WHERE id = @id", conn))
                        {
                            checkProductCmd.Parameters.AddWithValue("id", newProductId);
                            var count = (long)checkProductCmd.ExecuteScalar();
                            if (count > 0) break;
                            else Console.WriteLine("Продукт с таким ID не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ID продукта. ");
                    }
                }

                float newQuantity;
                while (true)
                {
                    Console.Write($"Количество (текущее: {currentQuantity}): ");
                    string? quantityInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(quantityInput))
                    {
                        newQuantity = currentQuantity;
                        break;
                    }

                    if (float.TryParse(quantityInput, out newQuantity) && newQuantity >= 0)
                        break;
                    Console.WriteLine("Количество должно быть неотрицательным числом. ");
                }

                using (var updateCmd = new NpgsqlCommand("SELECT update_stored_product(@id, @wid, @pid, @q)", conn))
                {
                    updateCmd.Parameters.AddWithValue("id", storedId);
                    updateCmd.Parameters.AddWithValue("wid", newWarehouseId);
                    updateCmd.Parameters.AddWithValue("pid", newProductId);
                    updateCmd.Parameters.AddWithValue("q", newQuantity);
                    updateCmd.ExecuteNonQuery();
                    Console.WriteLine("Информация обновлена.");
                }
            }
                break;
            
            case "4":
            {
                int delId;
                while (true)
                {
                    Console.Write("Введите ID хранимого продукта для удаления: ");
                    if (int.TryParse(Console.ReadLine(), out delId))
                    {
                        using (var checkCmd =
                               new NpgsqlCommand("SELECT COUNT(*) FROM stored_product WHERE id = @i", conn))
                        {
                            checkCmd.Parameters.AddWithValue("i", delId);
                            var count = (long)checkCmd.ExecuteScalar();
                            if (count > 0) break;
                            else Console.WriteLine("Хранимый продукт с таким ID не найден.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ID. ");
                    }
                }

                using (var delCmd = new NpgsqlCommand("SELECT delete_stored_product(@id)", conn))
                {
                    delCmd.Parameters.AddWithValue("id", delId);
                    delCmd.ExecuteNonQuery();
                    Console.WriteLine("Хранимый продукт удалён.");
                }
            }
                break;
            
            case "5":
            {
                int storedProductId;
                while (true)
                {
                    Console.Write("Введите ID хранимого продукта: ");
                    if (int.TryParse(Console.ReadLine(), out storedProductId))
                        break;
                    Console.WriteLine("Некорректный ID.");
                }

                using (var cmd = new NpgsqlCommand(
                           "SELECT stored_product_id, warehouse_type FROM get_warehouse_type_by_stored_product_id(@id)",
                           conn))
                {
                    cmd.Parameters.AddWithValue("id", storedProductId);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int spId = reader.GetInt32(0);
                        string warehouseType = reader.GetString(1);
                        Console.WriteLine($"ID хранимого продукта: {spId}, Тип склада: {warehouseType}");
                    }
                    else
                    {
                        Console.WriteLine("Хранимый продукт с таким ID не найден.");
                    }
                }
            }
                break;

            case "6":
            {
                using (var cmd = new NpgsqlCommand("SELECT * FROM get_all_products()", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    PrintTable(reader, new[] { 10, 30, 30 }, "ID", "Название продукта", "Тип продукта");
                }
            }
                break;

            case "7":
            {
                string newProductName;
                while (true)
                {
                    Console.Write("Введите название продукта: ");
                    newProductName = Console.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(newProductName))
                        break;
                    Console.WriteLine("Название продукта не может быть пустым.");
                }

                string newProductType;
                while (true)
                {
                    Console.Write("Введите тип продукта: ");
                    newProductType = Console.ReadLine()?.Trim() ?? "";
                    if (!string.IsNullOrWhiteSpace(newProductType))
                        break;
                    Console.WriteLine("Тип продукта не может быть пустым.");
                }

                using (var cmd = new NpgsqlCommand("SELECT insert_product(@name, @type)", conn))
                {
                    cmd.Parameters.AddWithValue("name", newProductName);
                    cmd.Parameters.AddWithValue("type", newProductType);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Продукт успешно добавлен.");
                }
            }
                break;

            case "8":
            {
                int productId;
                while (true)
                {
                    Console.Write("Введите ID продукта для редактирования: ");
                    if (int.TryParse(Console.ReadLine(), out productId))
                        break;
                    Console.WriteLine("Некорректный ID. ");
                }

                string currentName = "";
                string currentType = "";

                using (var getCmd = new NpgsqlCommand("SELECT product_name, product_type FROM product WHERE id = @id",
                           conn))
                {
                    getCmd.Parameters.AddWithValue("id", productId);
                    using var reader = getCmd.ExecuteReader();
                    if (reader.Read())
                    {
                        currentName = reader.GetString(0);
                        currentType = reader.GetString(1);
                    }
                    else
                    {
                        Console.WriteLine("Продукт с таким ID не найден.");
                        break;
                    }
                }

                string updatedName;
                while (true)
                {
                    Console.Write($"Новое название продукта (текущее: {currentName}): ");
                    updatedName = Console.ReadLine()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(updatedName))
                    {
                        updatedName = currentName;
                        break;
                    }
                    else break;
                }

                string updatedType;
                while (true)
                {
                    Console.Write($"Новый тип продукта (текущий: {currentType}): ");
                    updatedType = Console.ReadLine()?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(updatedType))
                    {
                        updatedType = currentType;
                        break;
                    }
                    else break;
                }

                using (var updateCmd = new NpgsqlCommand("SELECT update_product(@id, @name, @type)", conn))
                {
                    updateCmd.Parameters.AddWithValue("id", productId);
                    updateCmd.Parameters.AddWithValue("name", updatedName);
                    updateCmd.Parameters.AddWithValue("type", updatedType);
                    updateCmd.ExecuteNonQuery();
                    Console.WriteLine("Информация о продукте обновлена.");
                }
            }
                break;
            
            case "9":
            {
                int delProductId;
                while (true)
                {
                    Console.Write("Введите ID продукта для удаления: ");
                    if (int.TryParse(Console.ReadLine(), out delProductId))
                    {
                        using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM product WHERE id = @id", conn))
                        {
                            checkCmd.Parameters.AddWithValue("id", delProductId);
                            var count = (long)checkCmd.ExecuteScalar();
                            if (count > 0) break;
                            else Console.WriteLine("Продукт с таким ID не найден. ");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ID. ");
                    }
                }

                using (var delCmd = new NpgsqlCommand("SELECT delete_product(@id)", conn))
                {
                    delCmd.Parameters.AddWithValue("id", delProductId);
                    delCmd.ExecuteNonQuery();
                    Console.WriteLine("Продукт успешно удалён.");
                }
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