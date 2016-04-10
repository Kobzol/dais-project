using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.ORM;

namespace DatabaseTest
{
    [TestClass]
    public class ExamResultTest : BaseTest
    {
        [TestMethod]
        public void TestExamResult()
        {
            int userId = 1;

            QuestionAnswer answer1 = new QuestionAnswer("Ostrava", true);
            QuestionAnswer answer2 = new QuestionAnswer("Praha", false);
            Question question = QuestionTable.InsertQuestion(this.connection, userId, "test", "closed", new List<QuestionAnswer>() { answer1, answer2 });
            
            Exam exam = ExamTable.InsertExam(this.connection, userId, "test exam", 10, 1, 1, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            exam.AddQuestion(question.Id, 20);
            ExamTable.UpdateExam(this.connection, exam);

            int examResultId = ExamResultTable.InsertExamResult(this.connection, exam.Id, userId);
            ExamResult examResult = ExamResultTable.GetExamResultById(this.connection, examResultId);

            Assert.AreEqual(examResultId, examResult.Id);
            Assert.AreEqual(userId, examResult.OwnerId);
            Assert.AreEqual("created", examResult.State);

            ExamAnswer answer = new ExamAnswer(question.Id, "Ostrava");
            Assert.IsTrue(ExamResultTable.HandInExamResult(this.connection, examResult, new List<ExamAnswer>() { answer }));

            examResult = ExamResultTable.GetExamResultById(this.connection, examResult.Id);

            Assert.AreEqual("finished", examResult.State);
            Assert.AreEqual(exam.Questions[0].Points, examResult.Points);
        }
    }
}
