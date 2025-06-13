## ComputerClub2 🎮💻

**ComputerClub2** — десктопное приложение для управления компьютерным клубом, разработанное на C# с использованием WPF. Проект помогает автоматизировать регистрацию, вход, управление пользователями и администраторами, а также ведение базы клиентов и рабочих мест.

---

**Автор:** [ivankolt](https://github.com/ivankolt)  
**Язык:** C#  
**Платформа:** .NET/WPF  
**Статус:** В разработке 🚧

---

## Возможности ✨

- 🔐 Регистрация и авторизация пользователей и администраторов  
- 🖥️ Управление рабочими местами (ПК)  
- 📋 Ведение базы клиентов и посещений  
- 🧑‍💼 Интерфейс администратора и пользователя  
- 🗄️ Хранение данных в базе PostgreSQL (структура — `BD.sql`)  
- 🎨 Графический интерфейс на WPF  

---

## Используемая база данных 🐘

В проекте используется **PostgreSQL** как основная СУБД.  
Структура базы включает 17 таблиц:

- blocked_users
- bookings
- employees
- equipment
- orders
- orders_products
- payments
- pc
- pc_bookings
- persons
- posts
- products
- receipts
- received_products
- shifts
- user_actions
- users

![image](https://github.com/user-attachments/assets/34aafdd0-4be2-4fd1-82f9-c20b5bb5e9f5) Структура проекта 🗂️

- **Admin/** — компоненты для работы администратора
- **Manager/** — логика управления клубом
- **Registration/** — регистрация пользователей
- **Entrance/** — формы входа
- **Users/** — работа с пользователями
- **PC.cs** — описание рабочих мест
- **BD/** — база данных и скрипты
- **Image/**, **Images/** — ресурсы и иконки
- **App.xaml**, **MainWindow.xaml** — основной интерфейс
- **App.config** — конфигурация приложения

---

## Установка и запуск 🚀

1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/ivankolt/ComputerClub2.git
   ```
2. Откройте проект в Visual Studio.
3. Настройте подключение к базе данных PostgreSQL:
   - Установите PostgreSQL и создайте базу данных, используя скрипт `BD.sql`.
   - Проверьте строку подключения в `App.config`.
4. Соберите и запустите проект.

---

## Скриншоты 📸

![image](https://github.com/user-attachments/assets/a644a0b7-3118-4167-8980-a727046c15be)


![image](https://github.com/user-attachments/assets/d925c5cb-fb79-4e61-9dc8-b883d17f79c9)


![image](https://github.com/user-attachments/assets/669c39c3-24d9-40f4-957e-adb744545b7a)


![image](https://github.com/user-attachments/assets/6f044c2e-715e-4101-ae6b-38fe47b73a49)

---

## Вклад 🤝

Проект в разработке, предложения и пулл-реквесты приветствуются!  
Для обсуждения используйте Issues на GitHub.

---

## Лицензия 📄

Проект распространяется под лицензией **MIT** — одной из самых популярных и простых лицензий, разрешающей использовать, изменять и распространять код с минимальными ограничениями. Просто сохраняйте оригинальный текст лицензии и уведомление об авторских правах при распространении.

```
MIT License

Copyright (c) 2025 ivankolt

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

---

Удачи и приятного использования! 😊

[1] https://pplx-res.cloudinary.com/image/private/user_uploads/74081480/18e87330-6944-42b3-9e07-0bc82906ac7f/image.jpg
[2] https://support.syncfusion.com/kb/article/11107/how-to-perform-the-crud-operations-in-wpf-scheduler-calendar-using-postgresql-database
[3] https://learn.microsoft.com/en-us/dotnet/aspire/database/postgresql-integration
[4] https://neon.com/postgresql/postgresql-csharp/postgresql-csharp-connect
[5] https://tembo.io/docs/getting-started/postgres_guides/connecting-to-postgres-with-c-sharp
[6] https://michaelscodingspot.com/postgres-in-csharp/
[7] https://stackoverflow.com/questions/71713366/how-can-i-retrieve-data-from-my-postgresql-database-wpf-c
[8] https://www.connectionstrings.com/questions/100132/connection-to-postgresql-database-from-c-wpf-application-using-dapper/
[9] https://www.npgsql.org
[10] https://help.reg.ru/support/servery-vps/oblachnyye-bazy-dannykh/postgresql/kak-upravlyat-bazami-dannykh-v-postgresql
[11] https://blog.skillfactory.ru/glossary/postgresql/
