using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;

namespace Elearning.ORM
{
    public class ExamQuestion : TableRecord
    {
        [DBColumn("questionId", SqlDbType.Int)]
        public int QuestionId
        {
            get;
            private set;
        }

        [DBColumn("examId", SqlDbType.Int)]
        public int ExamId
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

        [DBColumn("points", SqlDbType.Decimal)]
        public decimal Points
        {
            get;
            set;
        }

        public ExamQuestion(int questionId, int examId, int index, decimal points)
        {
            this.QuestionId = questionId;
            this.ExamId = examId;
            this.Index = index;
            this.Points = points;
        }

        public Exam GetExam(DBConnection connection)
        {
            return ExamTable.GetExamById(connection, this.ExamId);
        }
    }
}
