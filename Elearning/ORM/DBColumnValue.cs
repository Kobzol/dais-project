using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.ORM
{
    public class DBColumnValue
    {
        public string Value
        {
            get;
            private set;
        }
        public DBColumnAttribute Attribute
        {
            get;
            private set;
        }

        public DBColumnValue(string value, DBColumnAttribute attribute)
        {
            this.Value = value;
            this.Attribute = attribute;
        }
    }
}
