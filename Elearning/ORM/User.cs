using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.ORM
{
    public class User : TableRecord
    {
        [DBColumn("userId", SqlDbType.Int)]
        public int Id
        {
            get;
            private set;
        }

        public Role Role
        {
            get;
            set;
        }

        [DBColumn("name", SqlDbType.VarChar, 100)]
        public string Name
        {
            get;
            set;
        }

        [DBColumn("surname", SqlDbType.VarChar, 100)]
        public string Surname
        {
            get;
            set;
        }

        [DBColumn("created_at", SqlDbType.DateTime2)]
        public DateTime CreationTime
        {
            get;
            private set;
        }

        public User(int id, Role role, string name, string surname, DateTime creationTime)
        {
            this.Id = id;
            this.Role = role;
            this.Name = name;
            this.Surname = surname;
            this.CreationTime = creationTime;
        }
    }
}
