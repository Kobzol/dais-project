using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.ORM;
using Elearning.Database;

namespace DatabaseTest
{
    [TestClass]
    public class ExamTest : BaseTest
    {
        [TestMethod]
        public void TestExamCreation()
        {
            Exam exam = ExamTable.InsertExam(this.connection, 1, "test", 10, 10, 3, DateTime.Now, DateTime.Now.AddDays(1));
            Exam dbExam = ExamTable.GetExamById(this.connection, exam.Id);

            Assert.AreEqual(exam.Id, dbExam.Id);
            Assert.AreEqual(exam.OwnerId, dbExam.OwnerId);
            Assert.AreEqual(exam.Name, dbExam.Name);
            Assert.AreEqual(exam.Timelimit, dbExam.Timelimit);
            Assert.AreEqual(exam.MinimumPoints, dbExam.MinimumPoints);
            Assert.AreEqual(exam.MaximumAttempts, dbExam.MaximumAttempts);
            Assert.AreEqual(exam.StartDate, dbExam.StartDate);
            Assert.AreEqual(exam.EndDate, dbExam.EndDate);

            Assert.IsTrue(ExamTable.DeleteExam(this.connection, exam, 1));

            try
            {
                exam = ExamTable.GetExamById(this.connection, exam.Id);
                Assert.Fail("Test pořád existuje");
            }
            catch (DatabaseException)
            {

            }
        }

        [TestMethod]
        public void TestExamActive()
        {
            Exam activeExam = ExamTable.InsertExam(this.connection, 1, "test", 10, 10, 10, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.IsTrue(ExamTable.IsExamActive(this.connection, activeExam));

            Exam inactiveExam = ExamTable.InsertExam(this.connection, 1, "test", 10, 10, 10, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2));
            Assert.IsFalse(ExamTable.IsExamActive(this.connection, inactiveExam));

            Assert.IsTrue(ExamTable.DeleteExam(this.connection, activeExam, activeExam.OwnerId));
            Assert.IsTrue(ExamTable.DeleteExam(this.connection, inactiveExam, inactiveExam.OwnerId));
        }

        [TestMethod]
        public void TestExamQuestions()
        {
            Exam exam = ExamTable.InsertExam(this.connection, 1, "test", 10, 10, 10, DateTime.Now, DateTime.Now.AddDays(1));
            exam.AddQuestion(new ExamQuestion(1, exam.Id, 0, 5.5m));

            ExamTable.UpdateExam(this.connection, exam);

            Exam dbExam = ExamTable.GetExamById(this.connection, exam.Id);

            Assert.AreEqual(exam.Questions.Count, dbExam.Questions.Count);
            Assert.AreEqual(exam.Questions[0].Points, dbExam.Questions[0].Points);

            Assert.IsTrue(ExamTable.DeleteExam(this.connection, exam, exam.OwnerId));
        }

        [TestMethod]
        public void TestExamActiveList()
        {
            Exam exam = ExamTable.InsertExam(this.connection, 1, "test", 10, 10, 10, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.IsTrue(ExamTable.GetActiveExams(this.connection).Contains(exam.Id));

            Assert.IsTrue(ExamTable.DeleteExam(this.connection, exam, exam.OwnerId));
        }
    }
}
