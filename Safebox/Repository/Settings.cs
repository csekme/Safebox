using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Safebox.Repository
{
    internal class Settings
    {
        public static string VALIDATION_MESSAGE = "Árvíztükörfúrógép";

        public static string getValue(string param)
        {
            Debug.WriteLine(string.Format("getValue({0})", param));
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "safebox.dat");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                String query = @"
                        SELECT param from settings where name = @Name
                ";

                SqliteCommand selectCommand = new SqliteCommand(query, db);
                selectCommand.Parameters.AddWithValue("@Name", param);

                SqliteDataReader data = selectCommand.ExecuteReader();

                if (data.Read())
                {
                    return data.GetString(0);
                }
            }
            return null;
        }

        public static void insertValue(string name, string value)
        {
            Debug.WriteLine(string.Format("setValue({0}, {1})", name, value));
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "safebox.dat");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                //passwords table
                String command = @"
                        insert into settings (name, param) values (@Name, @Value);
                ";

                SqliteCommand insertCommand = new SqliteCommand(command, db);
                insertCommand.Parameters.AddWithValue("@Name",      name);
                insertCommand.Parameters.AddWithValue("@Value",    value);

                insertCommand.ExecuteReader();

                db.Close();

            }
        }

    }
}
