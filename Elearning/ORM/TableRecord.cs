using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using Elearning.Database;

namespace Elearning.ORM
{
    public abstract class TableRecord
    {
        public static DBColumnAttribute GetAttribute(Type type, string name)
        {
            return type.GetProperties().First<PropertyInfo>((PropertyInfo prop) =>
            {
                return prop.Name == name;
            }).GetCustomAttribute<DBColumnAttribute>(false);
        }
        public static ColumnSet GetColumnSet(Type type)
        {
            List<DBColumnAttribute> attributes = (from attrib in type.GetProperties()
                                                 where attrib.GetCustomAttribute<DBColumnAttribute>(false) != null
                                                 select attrib.GetCustomAttribute<DBColumnAttribute>(false)).ToList<DBColumnAttribute>();

            return new ColumnSet(attributes);
        }
        public static DBColumnAttribute GetAttributeByName(Type type, string name)
        {
            return (from attrib in type.GetProperties()
                    where attrib.Name == name
                    select attrib.GetCustomAttribute<DBColumnAttribute>(false)).First();
        }

        public List<DBColumnValue> GetColumnValuesEscaped()
        {
            ColumnSet columnSet = TableRecord.GetColumnSet(this.GetType());
            List<DBColumnValue> values = new List<DBColumnValue>();

            foreach (DBColumnAttribute attrib in columnSet)
            {
                values.Add(new DBColumnValue(DatabaseHelper.DBEscape(this.GetType().GetProperty(attrib.PropertyName).GetValue(this)), attrib));
            }

            return values;
        }
        public DBColumnAttribute GetAttributeByName(string name)
        {
            return (from attrib in this.GetType().GetProperties()
                    where attrib.Name == name
                    select attrib.GetCustomAttribute<DBColumnAttribute>(false)).First();
        }
    }
}
