using Elearning.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elearning.ORM
{
    public class ExamAnswer : TableRecord
    {
        [DBColumn("examAnswerId", SqlDbType.Int)]
        public int Id
        {
            get;
            private set;
        }

        [DBColumn("examResultId", SqlDbType.Int)]
        public int ExamResultId
        {
            get;
            private set;
        }

        [DBColumn("questionId", SqlDbType.Int)]
        public int QuestionId
        {
            get;
            private set;
        }

        [DBColumn("text", SqlDbType.NVarChar, -1)]
        public string Text
        {
            get;
            private set;
        }

        [DBColumn("points", SqlDbType.Decimal)]
        public decimal Points
        {
            get;
            private set;
        }

        public ExamAnswer(int questionId, string text)
        {
            this.QuestionId = questionId;
            this.Text = text;
        }
        public ExamAnswer(int id, int examResultId, int questionId, string text, decimal points)
        {
            this.Id = id;
            this.ExamResultId = examResultId;
            this.QuestionId = questionId;
            this.Text = text;
            this.Points = points;
        }

        public ExamResult GetExamResult(DBConnection connection)
        {
            return ExamResultTable.GetExamResultById(connection, this.ExamResultId);
        }

        public Question GetQuestion(DBConnection connection)
        {
            return QuestionTable.GetQuestionById(connection, this.QuestionId);
        }
    }
}
