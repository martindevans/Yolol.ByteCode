using Yolol.Execution;

using static Yolol.ByteCode.Tests.TestHelpers;

namespace Yolol.ByteCode.Tests;

[TestClass]
public class Reproduction
{
    [TestMethod]
    public void RadStrings()
    {
        var ms = Test(new [] {
            "a=\"\" b=a-- y=2 goto0",
            "x=1"
        }, 3);

        Assert.AreEqual("", ms.GetVariable("a").ToString());
        Assert.AreEqual(0, (int)ms.GetVariable("b").Number);
        Assert.AreEqual(1, (int)ms.GetVariable("x").Number);
        Assert.AreEqual(0, (int)ms.GetVariable("y").Number);
    }


    [TestMethod]
    public void ZijkhalBlackFriday()
    {
        var result = Test([
            ":i=\"8591433801\" a=\"*********\"i=a+9p+=a goto++k/57",
            "h=a--+8 g=a--+7 f=a--+6 e=a--+5 d=a--+4 c=a--+3 b=a--+2 a=\"*1\"",
            "t=:i+:i q=p-0+t-a-b-c-d-e-f-g-h-i-0 s=q+t l=s-s--",
            "q=q+l-a-b-c-d-e-f-g-h-i-0 s=q+t m=s-s-- q=q+m-a-b-c-d-e-f-g-h-i-0 s=q+t+t",
            "n=s-s-- q=q+n-a-b-c-d-e-f-g-h-i-0 :done=1 s=q+t+t :o=l+m+n+(s-s--)",
        ], 200); // run for 200 ticks or once :done is true

        Assert.AreEqual("0000", result.GetVariable(":o").ToString());
    }

    [TestMethod]
    public void ZijkhalBlackFridayReduction3()
    {
        var result = TestHelpers.Test(new[] {
            ":i=\"8591433801\" a=\"*********\"i=a+9p+=a goto++k/57",
            "h=a--+8 g=a--+7 f=a--+6 e=a--+5 d=a--+4 c=a--+3 b=a--+2 a=\"*1\"",
            "t=:i+:i q=p-0+t-a-b-c-d-e-f-g-h-i-0 s=q+t l=s-s--",
            "q=q+l-a-b-c-d-e-f-g-h-i-0 s=q+t m=s-s-- q=q+m-a-b-c-d-e-f-g-h-i-0 s=q+t+t",
            //"n=s-s-- q=q+n-a-b-c-d-e-f-g-h-i-0 :done=1 s=q+t+t :o=l+m+n+(s-s--)"
        }, 116);

        Assert.AreEqual("8591433801", result.GetVariable(":i"));
        Assert.AreEqual("*1", result.GetVariable("a"));
        Assert.AreEqual("*********9", result.GetVariable("i"));
        Assert.AreEqual("0***************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************", result.GetVariable("p"));
        Assert.AreEqual((Value)114, result.GetVariable("k"));

        Assert.AreEqual("********8", result.GetVariable("h"));
        Assert.AreEqual("*******7", result.GetVariable("g"));
        Assert.AreEqual("******6", result.GetVariable("f"));
        Assert.AreEqual("*****5", result.GetVariable("e"));
        Assert.AreEqual("****4", result.GetVariable("d"));
        Assert.AreEqual("***3", result.GetVariable("c"));
        Assert.AreEqual("**2", result.GetVariable("b"));
        Assert.AreEqual("*1", result.GetVariable("a"));

        Assert.AreEqual("85914338018591433801", result.GetVariable("t"));
        Assert.AreEqual("*******************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************", result.GetVariable("q"));
        Assert.AreEqual("*******************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************************85914338", result.GetVariable("s"));
        Assert.AreEqual("0", result.GetVariable("l"));
    }

    [TestMethod]
    public void BoolMultiplyHuge()
    {
        var result = Test($"x=asin 1992768.34 c=1*x");

        Assert.AreEqual(-9223372036854775.808, (double)result.GetVariable("x").Number);
        Assert.AreEqual(0, (double)result.GetVariable("c").Number);
    }

    [TestMethod]
    public void NumberPop()
    {
        var result = Test(":a=11 :b=:a-:a--");

        Assert.AreEqual(10, (int)result.GetVariable(":a").Number);
        Assert.AreEqual(1, (int)result.GetVariable(":b").Number);
    }

    [TestMethod]
    public void NumberValPop()
    {
        var result = Test(new[] { ":a=11", ":b=:a-:a--" }, 2);

        Assert.AreEqual(10, (int)result.GetVariable(":a").Number);
        Assert.AreEqual(1, (int)result.GetVariable(":b").Number);
    }

    [TestMethod]
    public void StringPop()
    {
        var result = Test(":a=\"abc\" :b=:a-:a--");

        Assert.AreEqual("ab", result.GetVariable(":a").String.ToString());
        Assert.AreEqual("c", result.GetVariable(":b").String.ToString());
    }

    [TestMethod]
    public void StringPopErr()
    {
        var result = Test(":a=\"\" :b=:a-:a-- :c=1");

        Assert.AreEqual("", result.GetVariable(":a").String.ToString());
        Assert.AreEqual(0, (int)result.GetVariable(":b").Number);
        Assert.AreEqual(0, (int)result.GetVariable(":c").Number);
    }

    [TestMethod]
    public void StringValPop()
    {
        var result = Test(new[] { ":a=\"abc\"", ":b=:a-:a--" }, 2);

        Assert.AreEqual("ab", result.GetVariable(":a").String.ToString());
        Assert.AreEqual("c", result.GetVariable(":b").String.ToString());
    }

    [TestMethod]
    public void StringValPopErr()
    {
        var result = Test(new[] { ":a=\"\"", ":b=:a-:a-- :c=1" }, 2);

        Assert.AreEqual("", result.GetVariable(":a").String.ToString());
        Assert.AreEqual(0, (int)result.GetVariable(":b").Number);
        Assert.AreEqual(0, (int)result.GetVariable(":c").Number);
    }

    [TestMethod]
    public void GotoStringBug()
    {
        var ms = Test("b=\"tt\" :pi += (b-\"t\")==b goto 1");
        Assert.AreEqual(1, ms.ProgramCounter);
    }

    [TestMethod]
    public void NullRefString()
    {
        var ms = Test("c += \"t\"");
        Assert.AreEqual("0t", ms.GetVariable("c").String.ToString());
    }

    [TestMethod]
    public void NullRefString2()
    {
        var ms = Test("c = c + \"t\"");
        Assert.AreEqual("0t", ms.GetVariable("c").String.ToString());
    }

    [TestMethod]
    public void ConstantEvaluation()
    {
        var result = Test("a = \"2\" + 2 + 2");

        Assert.AreEqual("222", result.GetVariable("a").String.ToString());
    }

    [TestMethod]
    public void GotoDonePlusPlus()
    {
        var ms = Test("goto:done++");

        Assert.AreEqual(1, ms.ProgramCounter);
    }

    [TestMethod]
    public void UnreachableCode()
    {
        Test("if :i>8191 then :done=1 goto 1 end");
    }

    [TestMethod]
    public void NonStringSubtraction()
    {
        var ms = Test("a=70 b=\"\"+a-0");
        Assert.AreEqual(new YString("7"), ms.GetVariable("b").String);
    }

    [TestMethod]
    public void SomeoneLucasIf()
    {
        Test("if 7 then end");
    }

    [TestMethod]
    public void Abs()
    {
        var ms = Test("a = abs -7");
        Assert.AreEqual((Number)7, ms.GetVariable("a").Number);
    }

    [TestMethod]
    public void NotModulo()
    {
        Test(":o=not (:i%100)");
    }

    [TestMethod]
    public void Spaceship()
    {
        Test("if :q then :x=0 goto l end if :q then :x++ end");
    }

    [TestMethod]
    public void ZijkhalBoolDivision()
    {
        const string code = "a=-9223372036854775.808 c=a/1";
        var ast = Parse(code);

        var st = new MachineState(new NullDeviceNetwork(), 20);
        ast.Lines[0].Evaluate(1, st);

        var ms = Test(code);

        Assert.AreEqual(st.GetVariable("c").Value, ms.GetVariable("c"));
    }

    [TestMethod]
    public void CraterIncrement()
    {
        var ms = Test(":o=0 if v then :o++ end");
        Assert.AreEqual((Number)0, ms.GetVariable(":o").Number);
    }
}