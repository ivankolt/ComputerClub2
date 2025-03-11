//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace ComputerClub.BD
//{
//    internal class ВDConnect
//    {
//        static string BDConnecting = "server = localhost; user = root; password = Iv0708Iv0708; database = curssql";

//        static public MySqlDataAdapter MySqlDataAdapter;

//        static public MySqlCommand Command;

//        static public MySqlConnection Connection;

//        public static bool MyBDConnect()
//        {
//            try
//            {
//                Connection = new MySqlConnection(BDConnecting); /// Коннект с Базой данных

//                Connection.Open();//Открытие базы данных

//                Command = new MySqlCommand(); // создаём новый объект комманд

//                Command.Connection = Connection; // и подключаем его к нашему коннекту

//                return true;

//            }
//            catch
//            {
//                MessageBox.Show("Ошибка соеденения", "Ошибка", MessageBoxButton.OKCancel, MessageBoxImage.Error);
//                return false;
//            }
//        }

//        public static void Close()
//        {
//            Connection.Close();
//        }

//        public MySqlConnection MySqlConnection()
//        {

//            return Connection;
//        }
//    }
//}
