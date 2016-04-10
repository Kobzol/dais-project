using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.ORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DBColumnAttribute : Attribute
    {
        public string Name
        {
            get;
            private set;
        }
        public SqlDbType DbType
        {
            get;
            private set;
        }
        public int Size
        {
            get;
            private set;
        }
        public string PropertyName
        {
            get;
            private set;
        }

        public DBColumnAttribute(string name, SqlDbType dbType, [CallerMemberName] string propertyName = null) : this(name, dbType, 0, propertyName)
        {
            
        }
        public DBColumnAttribute(string name, SqlDbType dbType, int size, [CallerMemberName] string propertyName = null)
        {
            this.Name = name;
            this.DbType = dbType;
            this.Size = size;
            this.PropertyName = propertyName ?? String.Empty;
        }
    }
}
