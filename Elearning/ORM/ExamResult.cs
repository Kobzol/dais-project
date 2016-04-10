using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;

namespace Elearning.ORM
{
    public class ExamResult : TableRecord
    {
        [DBColumn("examResultId", SqlDbType.Int)]
        public int Id
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

        [DBColumn("ownerId", SqlDbType.Int)]
        public int OwnerId
        {
            get;
            private set;
        }

        [DBColumn("state", SqlDbType.VarChar, 255)]
        public string State
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

        [DBColumn("finished_at", SqlDbType.DateTime2)]
        public DateTime? FinishTime
        {
            get;
            private set;
        }

        [DBColumn("created_at", SqlDbType.DateTime2)]
        public DateTime CreationTime
        {
            get;
            private set;
        }

        public ExamResult(int id, int examId, int ownerId, string state, decimal points, DateTime creationTime, DateTime? finishTime = null)
        {
            this.Id = id;
            this.ExamId = examId;
            this.OwnerId = ownerId;
            this.State = state;
            this.Points = points;
            this.FinishTime = finishTime;
            this.CreationTime = creationTime;
        }

        public Exam GetExam(DBConnection connection)
        {
            return ExamTable.GetExamById(connection, this.ExamId);
        }

        public User GetOwner(DBConnection connection)
        {
            return UserTable.GetUserById(connection, this.OwnerId);
        }
    }
}
