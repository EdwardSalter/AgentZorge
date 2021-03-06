using Moq;

namespace TestNamespace
{
    public class MyTest
    {
        public void TestMethod(Mock<ITestService> testServiceMock)
        {
            testServiceMock.Setup(x => x.Call(It.IsAny<string>(), It.IsAny<int>())).Returns((string s, int i) => s + i).Callback((string s, int i) => { });
        }
    }

    public interface ITestService
    {
        string Call(string s, int i);
    }
}