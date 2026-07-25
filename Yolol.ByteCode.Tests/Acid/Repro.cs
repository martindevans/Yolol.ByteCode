using static Yolol.ByteCode.Tests.TestHelpers;

namespace Yolol.ByteCode.Tests.Acid;

[TestClass]
public class Repro
{
    [TestMethod]
    public void AcidStringLogic()
    {
        var ms = Test(new[] {
                "x = 1",
                "if false then end",
            },
            5
        );
    }
}