using Microsoft.Data.Sqlite;
using Safebox.Entities;
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
    internal class PasswordsRepo
    {

        public static IList<PasswordEntity> findAll()
        {
            IList<PasswordEntity> passwordEntities = new List<PasswordEntity>();
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "safebox.dat");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                String query = @"
                        SELECT * from passwords
                ";

                SqliteCommand selectCommand = new SqliteCommand(query, db);
                
                SqliteDataReader data = selectCommand.ExecuteReader();

                string u = AesHandler.memDec(App.user);
                string p = AesHandler.memDec(App.pswd);

                while (data.Read())
                {
                    PasswordEntity e = new PasswordEntity();
                    e.Id = data.GetInt16(data.GetOrdinal("id"));
                    e.Name = AesHandler.Decrypt(p, data.GetString(data.GetOrdinal("name")),u);
                    e.Username = AesHandler.Decrypt(p, data.GetString(data.GetOrdinal("username")),u);
                    e.Password = data.GetString(data.GetOrdinal("password")); //stay encoded
                    e.Uri = AesHandler.Decrypt(p, data.GetString(data.GetOrdinal("uri")),u);
                    e.Note = AesHandler.Decrypt(p, data.GetString(data.GetOrdinal("note")),u);
                    e.PrivateKey = data.GetString(data.GetOrdinal("private_key")); //stay encoded
                    passwordEntities.Add(e);    
                }
            }

            return passwordEntities;
        }

        public static void insert(PasswordEntity entity)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "safebox.dat");
            using (SqliteConnection db = new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                //passwords table
                String command = @"
                        insert into passwords (name, username, password, uri, note, private_key) 
                            values 
                        (@Name, @Username, @Password, @Uri, @Note, @PrivateKey);
                ";

                SqliteCommand insertCommand = new SqliteCommand(command, db);
                
                string u = AesHandler.memDec(App.user);
                string p = AesHandler.memDec(App.pswd);
                
                insertCommand.Parameters.AddWithValue("@Name", AesHandler.Encrypt(p, entity.Name, u));
                insertCommand.Parameters.AddWithValue("@Username", AesHandler.Encrypt(p, entity.Username, u));
                insertCommand.Parameters.AddWithValue("@Password", AesHandler.Encrypt(p, entity.Password, u));
                insertCommand.Parameters.AddWithValue("@Uri", AesHandler.Encrypt(p, entity.Uri, u));
                insertCommand.Parameters.AddWithValue("@Note", AesHandler.Encrypt(p, entity.Note, u));
                insertCommand.Parameters.AddWithValue("@PrivateKey", AesHandler.Encrypt(p, entity.PrivateKey, u));

                try
                {
                    insertCommand.ExecuteReader();
                } catch (SqliteException ex)
                {                    
                    Debug.WriteLine(ex.Message);
                }
                db.Close();

            }
        }

    }
}
