//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace ComputerClub.BD
//{
//    internal class CommandDB
//    {
//        static public string Role;

//        static public void Registr1(string Login, string Password, string Mail)
//        {
//            try
//            {
//                ВDConnect.Command.CommandText =
//                    @"Insert into people (Login, Password, Mail, Role) VALUES (@Login, @Password, @Mail, 'Пользователь')";
//                ВDConnect.Command.Parameters.AddWithValue("@Login", Login);
//                ВDConnect.Command.Parameters.AddWithValue("@Password", Password);
//                ВDConnect.Command.Parameters.AddWithValue("@Mail", Mail);
//                ВDConnect.Command.ExecuteNonQuery();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("Произошла ошибка при добавлении пользователя: " + ex.Message, "Ошибка",
//                    MessageBoxButton.OKCancel, MessageBoxImage.Error);
//            }

//        }

//        static public void Authorization(string Login, string Password)
//        {
//            try
//            {
//                if (ВDConnect.Command != null)
//                {
//                    ВDConnect.Command.CommandText =
//                        @"SELECT Role from people where Login = @Login and Password = @Password";
//                    ВDConnect.Command.Parameters.AddWithValue("@Login", Login);
//                    ВDConnect.Command.Parameters.AddWithValue("@Password", Password);
//                    Object Res = ВDConnect.Command.ExecuteScalar();

//                    if (Res != null)
//                    {
//                        Role = Res.ToString();
//                    }
//                    else
//                    {
//                        Role = null;
//                    }
//                }
//                else
//                {
//                    Role = null;
//                    MessageBox.Show("Произошла ошибка при подключении к базе данных.", "Ошибка",
//                        MessageBoxButton.OKCancel, MessageBoxImage.Error);
//                }
//            }
//            catch
//            {
//                Role = null;
//                MessageBox.Show("Такого пользователя нет", "Ошибка", MessageBoxButton.OKCancel,
//                    MessageBoxImage.Error);
//            }
//        }
//    }
//}
