using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;

namespace Elearning.ORM
{
    public class Question : TableRecord
    {
        [DBColumn("questionId", SqlDbType.Int)]
        public int Id
        {
            get;
            private set;
        }

        [DBColumn("ownerId", SqlDbType.Int)]
        public int OwnerId
        {
            get;
            private set;
        }

        [DBColumn("text", SqlDbType.NVarChar, -1)]
        public string Text
        {
            get;
            set;
        }

        [DBColumn("type", SqlDbType.VarChar, 10)]
        public string Type
        {
            get;
            set;
        }

        public List<QuestionAnswer> Answers
        {
            get;
            private set;
        }

        public Question(int id, int ownerId, string text, string type)
        {
            this.Id = id;
            this.OwnerId = ownerId;
            this.Text = text;
            this.Type = type;
            this.Answers = new List<QuestionAnswer>();
        }

        public void AddAnswer(QuestionAnswer answer)
        {
            this.Answers.Add(answer);
        }

        public User GetOwner(DBConnection connection)
        {
            return UserTable.GetUserById(connection, this.OwnerId);
        }
    }
}
