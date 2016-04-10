using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;

namespace Elearning.ORM
{
    public class QuestionAnswer : TableRecord
    {
        [DBColumn("questionId", SqlDbType.Int)]
        public int QuestionId
        {
            get;
            private set;
        }

        [DBColumn("index", SqlDbType.Int)]
        public int Index
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

        [DBColumn("correct", SqlDbType.Bit)]
        public bool Correct
        {
            get;
            set;
        }

        public QuestionAnswer(string text, bool correct)
        {
            this.Text = text;
            this.Correct = correct;
        }
        public QuestionAnswer(int questionId, int index, string text, bool correct)
        {
            this.QuestionId = questionId;
            this.Index = index;
            this.Text = text;
            this.Correct = correct;
        }

        public Question GetQuestion(DBConnection connection)
        {
            return QuestionTable.GetQuestionById(connection, this.QuestionId);
        }
    }
}
