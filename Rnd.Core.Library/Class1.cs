using System;
using System.Threading.Tasks;

namespace Rnd.Core.Library
{
    public class Class1
    {
        public async void TestMethod()
        {
            var test = 1;
        }
    }

    public class Class2
    {
        public void TestAction(Action<object> func)
        {

        }

        public void TestAsyncAction()
        {
            TestAction(async obj => await Task.CompletedTask);
        }
    }
}
