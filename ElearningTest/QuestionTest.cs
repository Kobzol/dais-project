using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elearning.Database;
using Elearning.Properties;
using Elearning.ORM;

namespace DatabaseTest
{
    [TestClass]
    public class QuestionTest : BaseTest
    {
        [TestMethod]
        public void TestQuestionInsert()
        {
            QuestionAnswer qaTest = new QuestionAnswer("Praha", true);
            QuestionAnswer qaTest2 = new QuestionAnswer("Ostrava", false);
            List<QuestionAnswer> questionList = new List<QuestionAnswer>() { qaTest, qaTest2 };

            Question generatedQuestion = QuestionTable.InsertQuestion(this.connection, 1, "Jaké je hlavní město ČR?", "closed", questionList);
            Question dbQuestion = QuestionTable.GetQuestionById(this.connection, generatedQuestion.Id);

            Assert.AreEqual(generatedQuestion.Id, dbQuestion.Id);
            Assert.AreEqual(generatedQuestion.OwnerId, dbQuestion.OwnerId);
            Assert.AreEqual(generatedQuestion.Text, dbQuestion.Text);
            Assert.AreEqual(generatedQuestion.Type, dbQuestion.Type);

            Assert.AreEqual(questionList.Count, dbQuestion.Answers.Count);

            for (int i = 0; i < dbQuestion.Answers.Count; i++)
            {
                Assert.AreEqual(dbQuestion.Id, dbQuestion.Answers[i].QuestionId);
                Assert.AreEqual(i, dbQuestion.Answers[i].Index);
                Assert.AreEqual(questionList[i].Text, dbQuestion.Answers[i].Text);
                Assert.AreEqual(questionList[i].Correct, dbQuestion.Answers[i].Correct);
            }

            Assert.IsTrue(QuestionTable.DeleteQuestion(this.connection, dbQuestion));

            try
            {
                QuestionTable.GetQuestionById(this.connection, dbQuestion.Id);
                Assert.Fail("Otázka stále existuje");
            }
            catch (DatabaseException)
            {

            }
        }

        [TestMethod]
        public void TestQuestionUpdate()
        {
            QuestionAnswer qaTest = new QuestionAnswer("Praha", true);
            QuestionAnswer qaTest2 = new QuestionAnswer("Ostrava", false);
            List<QuestionAnswer> questionList = new List<QuestionAnswer>() { qaTest, qaTest2 };

            Question generatedQuestion = QuestionTable.InsertQuestion(this.connection, 1, "Jaké je hlavní město ČR?", "closed", questionList);

            generatedQuestion.AddAnswer(new QuestionAnswer("Brno", false));
            generatedQuestion.Text = "Test";

            Assert.IsTrue(QuestionTable.UpdateQuestion(this.connection, generatedQuestion));

            Question updatedQuestion = QuestionTable.GetQuestionById(this.connection, generatedQuestion.Id);

            Assert.AreEqual("Test", updatedQuestion.Text);
            Assert.AreEqual(3, updatedQuestion.Answers.Count);
            Assert.AreEqual("Brno", updatedQuestion.Answers[2].Text);

            Assert.IsTrue(QuestionTable.DeleteQuestion(this.connection, generatedQuestion));
        }
    }
}
