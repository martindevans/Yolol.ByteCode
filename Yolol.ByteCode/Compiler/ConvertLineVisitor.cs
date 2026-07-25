using Yolol.Analysis.ControlFlowGraph.AST;
using Yolol.Analysis.TreeVisitor;
using Yolol.ByteCode.Instructions;
using Yolol.Execution;
using Yolol.Grammar.AST;
using Yolol.Grammar.AST.Expressions;
using Yolol.Grammar.AST.Expressions.Binary;
using Yolol.Grammar.AST.Expressions.Unary;
using Yolol.Grammar.AST.Statements;

namespace Yolol.ByteCode.Compiler;

public class ConvertLineVisitor
    : BaseTreeVisitor
{
    private readonly Emitter _emitter;

    private readonly int _maxLineNumber;

    public ConvertLineVisitor(List<Instruction> output, List<Value> constants, InternalsMap internals, ExternalsMap externals, int maxLineNumber, Dictionary<Emitter.LabelId, int> labels)
    {
        _maxLineNumber = maxLineNumber;
        _emitter = new Emitter(output, constants, internals, externals, labels);
    }

    public override Line Visit(Line line)
    {
        base.Visit(line);
        _emitter.EmitEol();
        return line;
    }

    #region statements

    protected override BaseStatement Visit(Assignment ass)
    {
        // Place the value to put into this variable on the stack
        if (Visit(ass.Right) is ErrorExpression)
            return ass;

        // Emit code to assign the value on the stack to the variable
        _emitter.EmitAssign(ass.Left);

        return ass;
    }

    protected override BaseStatement Visit(If @if)
    {
        // Create labels for control flow like:
        //
        //     entry point
        //     branch_if_false falseLabel
        //     true branch code
        //     jmp exitLabel
        //     falseLabel:
        //         false branch code
        //     exitlabel:
        //
        var falseLabel = _emitter.DefineLabel();
        var exitLabel = _emitter.DefineLabel();

        // Visit conditional which places a value on the stack
        if (Visit(@if.Condition) is ErrorExpression)
            return @if;

        // jump to false branch if the condition is false. Fall through to true branch
        _emitter.EmitBranchIfFalse(falseLabel);

        // Emit true branch
        Visit(@if.TrueBranch);
        _emitter.EmitBranch(exitLabel);

        // Emit false branch
        _emitter.MarkLabel(falseLabel);
        Visit(@if.FalseBranch);

        // Exit point for both branches
        _emitter.MarkLabel(exitLabel);

        return @if;
    }

    protected override BaseStatement Visit(Goto @goto)
    {
        // Put destination value on the stack
        if (@goto.Destination is ConstantNumber constNum)
        {
            var value = Math.Clamp((int)constNum.Value, 1, _maxLineNumber);
            var cid = _emitter.CreateConstant((Number)value);
            _emitter.EmitLoadConstant(cid);
        }
        else
        {
            if (Visit(@goto.Destination) is ErrorExpression)
                return @goto;
        }

        _emitter.EmitGoto();

        return @goto;
    }

    protected override BaseStatement Visit(CompoundAssignment compAss) => Visit(new Assignment(compAss.Left, compAss.Right));

    #endregion

    #region basic expressions

    protected override BaseExpression Visit(ConstantNumber con)
    {
        var cid = _emitter.CreateConstant(con.Value);
        _emitter.EmitLoadConstant(cid);

        return con;
    }

    protected override BaseExpression Visit(ConstantString str)
    {
        var cid = _emitter.CreateConstant(new Value(str.Value));
        _emitter.EmitLoadConstant(cid);

        return str;
    }

    protected override BaseExpression Visit(Grammar.AST.Expressions.Variable var)
    {
        base.Visit(var);
        _emitter.EmitLoadVariable(var.Name);
        return var;
    }

    #endregion

    #region binary expressions

    protected override BaseExpression Visit(Add add)
    {
        base.Visit(add);
        _emitter.EmitAdd();
        return add;
    }

    protected override BaseExpression Visit(Subtract sub)
    {
        base.Visit(sub);
        _emitter.EmitSubtract();
        return sub;
    }

    protected override BaseExpression Visit(Multiply mul)
    {
        base.Visit(mul);
        _emitter.EmitMultiply();
        return mul;
    }

    protected override BaseExpression Visit(Divide div)
    {
        base.Visit(div);
        _emitter.EmitDivide();
        return div;
    }

    protected override BaseExpression Visit(EqualTo eq)
    {
        base.Visit(eq);
        _emitter.EmitEqualTo();
        return eq;
    }

    protected override BaseExpression Visit(NotEqualTo neq)
    {
        base.Visit(neq);
        _emitter.EmitNotEqualTo();
        return neq;
    }

    protected override BaseExpression Visit(GreaterThan gt)
    {
        base.Visit(gt);
        _emitter.EmitGreaterThan();
        return gt;
    }

    protected override BaseExpression Visit(GreaterThanEqualTo gteq)
    {
        base.Visit(gteq);
        _emitter.EmitGreaterThanEqualTo();
        return gteq;
    }

    protected override BaseExpression Visit(LessThan lt)
    {
        base.Visit(lt);
        _emitter.EmitLessThan();
        return lt;
    }

    protected override BaseExpression Visit(LessThanEqualTo lteq)
    {
        base.Visit(lteq);
        _emitter.EmitLessThanEqualTo();
        return lteq;
    }

    protected override BaseExpression Visit(Modulo mod)
    {
        base.Visit(mod);
        _emitter.EmitModulo();
        return mod;
    }

    protected override BaseExpression Visit(And and)
    {
        base.Visit(and);
        _emitter.EmitAnd();
        return and;
    }

    protected override BaseExpression Visit(Or or)
    {
        base.Visit(or);
        _emitter.EmitOr();
        return or;
    }

    protected override BaseExpression Visit(Exponent exp)
    {
        base.Visit(exp);
        _emitter.EmitExponent();
        return exp;
    }

    #endregion

    #region unary expressions

    //public override BaseExpression Visit(BaseExpression expression)
    //{
    //    if (expression is BaseUnaryExpression u)
    //    {
    //        Visit(u.Parameter);
    //    }
    //    else if (expression is BaseBinaryExpression b)
    //    {
    //        Visit(b.Left);
    //        Visit(b.Right);
    //    }

    //    return base.Visit(expression);
    //}

    protected override BaseExpression Visit(Factorial fac)
    {
        base.Visit(fac);
        _emitter.EmitFactorial();
        return fac;
    }

    protected override BaseExpression Visit(Not not)
    {
        base.Visit(not);
        _emitter.EmitNot();
        return not;
    }

    protected override BaseExpression Visit(Negate neg)
    {
        base.Visit(neg);
        _emitter.EmitNegate();
        return neg;
    }

    protected override BaseExpression Visit(Sqrt sqrt)
    {
        base.Visit(sqrt);
        _emitter.EmitSqrt();
        return sqrt;
    }

    protected override BaseExpression Visit(ArcCos acos)
    {
        base.Visit(acos);
        _emitter.EmitArcCos();
        return acos;
    }

    protected override BaseExpression Visit(ArcSine asin)
    {
        base.Visit(asin);
        _emitter.EmitArcSin();
        return asin;
    }

    protected override BaseExpression Visit(ArcTan atan)
    {
        base.Visit(atan);
        _emitter.EmitArcTan();
        return atan;
    }

    protected override BaseExpression Visit(Cosine cos)
    {
        base.Visit(cos);
        _emitter.EmitCos();
        return cos;
    }

    protected override BaseExpression Visit(Sine sin)
    {
        base.Visit(sin);
        _emitter.EmitSin();
        return sin;
    }

    protected override BaseExpression Visit(Tangent tan)
    {
        base.Visit(tan);
        _emitter.EmitTan();
        return tan;
    }

    protected override BaseExpression Visit(Abs abs)
    {
        base.Visit(abs);
        _emitter.EmitAbs();
        return abs;
    }

    #endregion

    #region modify expressions

    protected override BaseExpression Visit(PreIncrement inc)
    {
        _emitter.EmitPreIncrement(inc.Name);
        return inc;
    }

    protected override BaseExpression Visit(PreDecrement dec)
    {
        _emitter.EmitPreDecrement(dec.Name);
        return dec;
    }

    protected override BaseExpression Visit(PostIncrement inc)
    {
        // Deliberately the wrong increment type! Maintains compatibility with base Yolol.
        _emitter.EmitPreIncrement(inc.Name);
        return inc;
    }

    protected override BaseExpression Visit(PostDecrement dec)
    {
        // Deliberately the wrong decrement type! Maintains compatibility with base Yolol.
        _emitter.EmitPreDecrement(dec.Name);
        return dec;
    }

    #endregion

    protected override BaseExpression Visit(Bracketed brk)
        => Visit(brk.Parameter);

    protected override BaseStatement Visit(ExpressionWrapper expr)
    {
        var r = base.Visit(expr);

        // The wrapped expression left a value on the stack. Pop it off now.
        _emitter.Pop();

        return r;
    }
}