using Moq;

namespace TestNamespace
{
    public class MyTest
    {
        public void TestMethod(Mock<ITestService> testServiceMock)
        {
            testServiceMock.Setup(x => x.Call()).Callback(() => { });
        }
    }

    public interface ITestService
    {
        void Call();
    }
}