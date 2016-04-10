using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elearning.Database;

namespace Elearning.ORM
{
    public class Exam : TableRecord
    {
        [DBColumn("examId", SqlDbType.Int)]
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

        [DBColumn("name", SqlDbType.VarChar, 255)]
        public string Name
        {
            get;
            set;
        }

        [DBColumn("timelimit", SqlDbType.Int)]
        public int Timelimit
        {
            get;
            set;
        }

        [DBColumn("minimum_points", SqlDbType.Int)]
        public int MinimumPoints
        {
            get;
            set;
        }

        [DBColumn("maximum_attempts", SqlDbType.Int)]
        public int MaximumAttempts
        {
            get;
            set;
        }

        [DBColumn("start_date", SqlDbType.DateTime2)]
        public DateTime StartDate
        {
            get;
            set;
        }

        [DBColumn("end_date", SqlDbType.DateTime2)]
        public DateTime EndDate
        {
            get;
            set;
        }

        [DBColumn("created_at", SqlDbType.DateTime2)]
        public DateTime CreationTime
        {
            get;
            private set;
        }

        public List<ExamQuestion> Questions
        {
            get;
            private set;
        }

        public Exam(int id, int ownerId, string name, int timelimit, int minimum_points, int maximum_attempts, DateTime startDate, DateTime endDate, DateTime creationTime)
        {
            this.Id = id;
            this.OwnerId = ownerId;
            this.Name = name;
            this.Timelimit = timelimit;
            this.MinimumPoints = minimum_points;
            this.MaximumAttempts = maximum_attempts;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.CreationTime = creationTime;
            this.Questions = new List<ExamQuestion>();
        }

        public void AddQuestion(ExamQuestion examQuestion)
        {
            this.Questions.Add(examQuestion);
        }
        public void AddQuestion(int questionId, decimal points)
        {
            this.Questions.Add(new ExamQuestion(questionId, this.Id, this.Questions.Count, points));
        }

        public User GetOwner(DBConnection connection)
        {
            return UserTable.GetUserById(connection, this.OwnerId);
        }
    }
}
