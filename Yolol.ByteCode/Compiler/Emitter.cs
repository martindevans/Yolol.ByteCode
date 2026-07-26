using Yolol.ByteCode.Instructions;
using Yolol.Execution;
using Yolol.Grammar;

namespace Yolol.ByteCode.Compiler;

public class Emitter
{
    private readonly List<Instruction> _output;
    private readonly List<Value> _constants;
    private readonly InternalsMap _internals;
    private readonly ExternalsMap _externals;
    private readonly Dictionary<LabelId, int> _labelIndices;

    public Emitter(List<Instruction> output, List<Value> constants, InternalsMap internals, ExternalsMap externals, Dictionary<LabelId, int> labelIndices)
    {
        _output = output;
        _constants = constants;
        _internals = internals;
        _externals = externals;
        _labelIndices = labelIndices;
    }

    public byte CreateConstant(Value value)
    {
        var idx = _constants.IndexOf(value);
        if (idx != -1)
            return checked((byte)idx);

        _constants.Add(value);
        return checked((byte)(_constants.Count - 1));
    }

    public void EmitAssign(VariableName name)
    {
        if (name.IsExternal)
            _output.Add(Instruction.AssignExternal(_externals[name]));
        else
            _output.Add(Instruction.AssignInternal(_internals[name]));
    }

    public void EmitGoto()
    {
        _output.Add(Instruction.Goto());
    }

    public void EmitLoadConstantNumber(byte cid)
    {
        _output.Add(Instruction.LoadConstNum(cid));
    }

    public void EmitLoadConstantString(byte cid)
    {
        _output.Add(Instruction.LoadConstString(cid));
    }

    public void EmitLoadVariable(VariableName name)
    {
        if (name.IsExternal)
            _output.Add(Instruction.LoadExternal(_externals[name]));
        else
            _output.Add(Instruction.LoadInternal(_internals[name]));
    }

    public void EmitBranchIfFalse(Label label)
    {
        _output.Add(Instruction.BranchIfFalse(label.Id.Id));
    }

    public void EmitBranch(Label label)
    {
        _output.Add(Instruction.Branch(label.Id.Id));
    }

    public void EmitAdd()
    {
        _output.Add(Instruction.Add());
    }

    public void EmitSubtract()
    {
        _output.Add(Instruction.Subtract());
    }

    public void EmitMultiply()
    {
        _output.Add(Instruction.Multiply());
    }

    public void EmitDivide()
    {
        _output.Add(Instruction.Divide());
    }

    public void EmitEqualTo()
    {
        _output.Add(Instruction.EqualTo());
    }
    
    public void EmitNotEqualTo()
    {
        _output.Add(Instruction.NotEqualTo());
    }
    
    public void EmitGreaterThanEqualTo()
    {
        _output.Add(Instruction.GreaterThanEqualTo());
    }
    
    public void EmitGreaterThan()
    {
        _output.Add(Instruction.GreaterThan());
    }

    public void EmitLessThanEqualTo()
    {
        _output.Add(Instruction.LessThanEqualTo());
    }

    public void EmitLessThan()
    {
        _output.Add(Instruction.LessThan());
    }

    public void EmitModulo()
    {
        _output.Add(Instruction.Modulo());
    }

    public void EmitAnd()
    {
        _output.Add(Instruction.And());
    }

    public void EmitOr()
    {
        _output.Add(Instruction.Or());
    }

    public void EmitNot()
    {
        _output.Add(Instruction.Not());
    }

    public void EmitExponent()
    {
        _output.Add(Instruction.Exponent());
    }

    public void EmitFactorial()
    {
        _output.Add(Instruction.Factorial());
    }

    public void EmitNegate()
    {
        _output.Add(Instruction.Negate());
    }

    public void EmitSqrt()
    {
        _output.Add(Instruction.Sqrt());
    }

    public void EmitArcCos()
    {
        _output.Add(Instruction.Acos());
    }

    public void EmitArcSin()
    {
        _output.Add(Instruction.Asin());
    }

    public void EmitArcTan()
    {
        _output.Add(Instruction.Atan());
    }

    public void EmitCos()
    {
        _output.Add(Instruction.Cos());
    }

    public void EmitSin()
    {
        _output.Add(Instruction.Sin());
    }

    public void EmitTan()
    {
        _output.Add(Instruction.Tan());
    }

    public void EmitAbs()
    {
        _output.Add(Instruction.Abs());
    }
    
    public void EmitPreIncrement(VariableName name)
    {
        if (name.IsExternal)
            _output.Add(Instruction.PreIncExternal(_externals[name]));
        else
            _output.Add(Instruction.PreIncInternal(_internals[name]));
    }

    public void EmitPreDecrement(VariableName name)
    {
        if (name.IsExternal)
            _output.Add(Instruction.PreDecExternal(_externals[name]));
        else
            _output.Add(Instruction.PreDecInternal(_internals[name]));
    }
    
    public void EmitPop()
    {
        _output.Add(Instruction.Pop());
    }

    public void EmitEol()
    {
        _output.Add(Instruction.Eol());
    }

    public void EmitRuntimeError()
    {
        _output.Add(Instruction.RuntimeError());
    }

    #region labels
    private byte _nextLabel;
    
    public Label DefineLabel()
    {
        return new Label(new LabelId(checked(_nextLabel++)));
    }

    public void MarkLabel(Label label)
    {
        if (_labelIndices.ContainsKey(label.Id))
            throw new InvalidOperationException("Cannot mark label twice");

        _labelIndices[label.Id] = _output.Count;
    }

    public record Label(LabelId Id);

    public record struct LabelId(byte Id);
    #endregion
}