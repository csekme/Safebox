using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safebox.Entities
{
    public class PasswordEntity  
    {
        int id;
        string name;
        string username;
        string password;
        string uri;
        string note;
        string privateKey;

        public int Id { get { return id; } set { id = value; } }
        public string Name { set { name = value;  } get { return name; } }
        public string Username { set { username = value; } get { return username;} }
        public string Password { set { password = value; } get { return password; } }
        public string Uri { set { uri = value;  } get { return uri; } }
        public string Note { set { note = value; } get { return note; } }
        public string PrivateKey { set { privateKey = value; } get { return privateKey; } }

        public override bool Equals(object obj)
        {
            return obj is PasswordEntity entity &&
                   id == entity.id;
        }

        public override string ToString()
        {
            return name;
        }

    
    }
}
