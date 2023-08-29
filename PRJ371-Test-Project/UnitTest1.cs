using PRJ371_Project;

namespace PRJ371_Test_Project
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void Test1()
        {
            Test testScript = new Test();
            string words = testScript.sayHello(1);
            Assert.Equals(words, "Hello to me!");
            //Assert.Pass();
        }
    }
}