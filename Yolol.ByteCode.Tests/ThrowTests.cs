using Yolol.Execution;
using static Yolol.ByteCode.Tests.TestHelpers;

namespace Yolol.ByteCode.Tests;

[TestClass]
public class ThrowTests
{
    [TestMethod]
    public void DivZero()
    {
        var ms = Test(new[] {
            "err=0 a=10 b=0 c=a/b goto 3",
            "err=1 goto 2",
            "err=2 done=1 goto 3"
        }, 3);

        Assert.AreEqual(Number.One, ms.GetVariable("err").Number);
    }
}