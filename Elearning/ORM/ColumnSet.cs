using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.ORM
{
    public class ColumnSet : IEnumerable<DBColumnAttribute>
    {
        private readonly Dictionary<string, DBColumnAttribute> columnMap = new Dictionary<string, DBColumnAttribute>();
        private readonly List<DBColumnAttribute> attributes;

        public DBColumnAttribute this[string name]
        {
            get
            {
                return this.columnMap[name];
            }
        }
        public DBColumnAttribute this[int index]
        {
            get
            {
                return this.attributes[index];
            }
        }

        public ColumnSet(List<DBColumnAttribute> columnAttributes)
        {
            this.attributes = new List<DBColumnAttribute>(columnAttributes);

            for (int i = 0; i < columnAttributes.Count; i++)
            {
                columnMap[columnAttributes[i].PropertyName] = columnAttributes[i];
            }
        }

        public IEnumerator<DBColumnAttribute> GetEnumerator()
        {
            return this.attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.attributes.GetEnumerator();
        }
    }
}
