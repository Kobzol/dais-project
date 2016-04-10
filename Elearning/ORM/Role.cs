using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.ORM;
using Elearning.Database;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;

namespace Elearning.ORM
{
    public class Role : TableRecord
    {
        [DBColumn("roleId", SqlDbType.Int)]
        public int Id
        {
            get;
            private set;
        }

        [DBColumn("name", SqlDbType.VarChar, 255)]
        public string Name
        {
            get;
            set;
        }

        public Role(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
