using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Yolol.ByteCode.Instructions;

[StructLayout(LayoutKind.Explicit)]
[DebuggerDisplay("{Op}")]
public struct Instruction
{
    [FieldOffset(0)] public ushort RawValue;
    
    [FieldOffset(0)] public Op Op;
    [FieldOffset(1)] public byte Operand;

    public static Instruction AssignExternal(byte operand)
    {
        return new Instruction { Op = Op.AssignExternal, Operand = operand };
    }

    public static Instruction AssignInternal(byte operand)
    {
        return new Instruction { Op = Op.AssignInternal, Operand = operand };
    }

    public static Instruction Goto()
    {
        return new Instruction { Op = Op.Goto, Operand = 0 };
    }

    public static Instruction LoadConstString(byte cid)
    {
        return new Instruction { Op = Op.LoadConstString, Operand = cid };
    }

    public static Instruction LoadConstNum(byte cid)
    {
        return new Instruction { Op = Op.LoadConstNum, Operand = cid };
    }

    public static Instruction LoadExternal(byte external)
    {
        return new Instruction { Op = Op.LoadExternal, Operand = external };
    }
    
    public static Instruction LoadInternal(byte external)
    {
        return new Instruction { Op = Op.LoadInternal, Operand = external };
    }

    public static Instruction BranchIfFalse(byte labelId)
    {
        return new Instruction { Op = Op.BranchIfFalse, Operand = labelId };
    }

    public static Instruction Branch(byte labelId)
    {
        return new Instruction { Op = Op.Branch, Operand = labelId };
    }

    public static Instruction Add()
    {
        return new Instruction { Op = Op.Add, Operand = 0 };
    }

    public static Instruction Subtract()
    {
        return new Instruction { Op = Op.Sub, Operand = 0 };
    }

    public static Instruction Multiply()
    {
        return new Instruction { Op = Op.Mul, Operand = 0 };
    }

    public static Instruction Divide()
    {
        return new Instruction { Op = Op.Div, Operand = 0 };
    }
    
    public static Instruction DivideSmallInt(byte value)
    {
        return new Instruction { Op = Op.DivSmallInt, Operand = value };
    }

    public static Instruction Modulo()
    {
        return new Instruction { Op = Op.Mod, Operand = 0 };
    }

    public static Instruction ModuloSmallInt(byte value)
    {
        return new Instruction { Op = Op.ModSmallInt, Operand = value };
    }

    public static Instruction Exponent()
    {
        return new Instruction { Op = Op.Expo, Operand = 0 };
    }

    public static Instruction EqualTo()
    {
        return new Instruction { Op = Op.Eq, Operand = 0 };
    }
    
    public static Instruction EqualToSmallInt(byte value)
    {
        return new Instruction { Op = Op.EqSmallInt, Operand = value };
    }

    public static Instruction NotEqualTo()
    {
        return new Instruction { Op = Op.Neq, Operand = 0 };
    }
    
    public static Instruction GreaterThan()
    {
        return new Instruction { Op = Op.Gt, Operand = 0 };
    }

    public static Instruction GreaterThanEqualTo()
    {
        return new Instruction { Op = Op.Gteq, Operand = 0 };
    }

    public static Instruction LessThan()
    {
        return new Instruction { Op = Op.Lt, Operand = 0 };
    }

    public static Instruction LessThanEqualTo()
    {
        return new Instruction { Op = Op.Lteq, Operand = 0 };
    }

    public static Instruction And()
    {
        return new Instruction { Op = Op.And, Operand = 0 };
    }

    public static Instruction Or()
    {
        return new Instruction { Op = Op.Or, Operand = 0 };
    }

    public static Instruction Not()
    {
        return new Instruction { Op = Op.Not, Operand = 0 };
    }

    public static Instruction Abs()
    {
        return new Instruction { Op = Op.Abs, Operand = 0 };
    }

    public static Instruction Negate()
    {
        return new Instruction { Op = Op.Neg, Operand = 0 };
    }

    public static Instruction Factorial()
    {
        return new Instruction { Op = Op.Fac, Operand = 0 };
    }

    public static Instruction Sqrt()
    {
        return new Instruction { Op = Op.Sqrt, Operand = 0 };
    }

    public static Instruction Cos()
    {
        return new Instruction { Op = Op.Cos, Operand = 0 };
    }

    public static Instruction Sin()
    {
        return new Instruction { Op = Op.Sin, Operand = 0 };
    }

    public static Instruction Tan()
    {
        return new Instruction { Op = Op.Tan, Operand = 0 };
    }

    public static Instruction Acos()
    {
        return new Instruction { Op = Op.Acos, Operand = 0 };
    }

    public static Instruction Asin()
    {
        return new Instruction { Op = Op.Asin, Operand = 0 };
    }

    public static Instruction Atan()
    {
        return new Instruction { Op = Op.Atan, Operand = 0 };
    }

    public static Instruction Pop()
    {
        return new Instruction { Op = Op.Pop, Operand = 0 };
    }

    public static Instruction Eol()
    {
        return new Instruction { Op = Op.EndOfLine, Operand = 0 };
    }
    
    public static Instruction PreIncExternal(byte vid)
    {
        return new Instruction { Op = Op.PreIncExternal, Operand = vid };
    }

    public static Instruction PreIncInternal(byte vid)
    {
        return new Instruction { Op = Op.PreIncInternal, Operand = vid };
    }

    public static Instruction PreDecExternal(byte vid)
    {
        return new Instruction { Op = Op.PreDecExternal, Operand = vid };
    }

    public static Instruction PreDecInternal(byte vid)
    {
        return new Instruction { Op = Op.PreDecInternal, Operand = vid };
    }

    public static Instruction PreIncVarExternal(byte vid)
    {
        return new Instruction { Op = Op.IncVarExternal, Operand = vid };
    }

    public static Instruction PreIncVarInternal(byte vid)
    {
        return new Instruction { Op = Op.IncVarInternal, Operand = vid };
    }

    public static Instruction PreDecVarExternal(byte vid)
    {
        return new Instruction { Op = Op.DecVarExternal, Operand = vid };
    }

    public static Instruction PreDecVarInternal(byte vid)
    {
        return new Instruction { Op = Op.DecVarInternal, Operand = vid };
    }

    public static Instruction RuntimeError()
    {
        return new Instruction { Op = Op.RuntimeError, Operand = 0 };
    }
}

public enum Op
    : byte
{
    /// <summary>
    /// Marks the end of a line. Causes the interpreter to break execution.
    /// </summary>
    EndOfLine,

    /// <summary>
    /// Take value from execution stack and store to an external variable
    /// </summary>
    AssignExternal,
    
    /// <summary>
    /// Take value from execution stack and store to an internal variable
    /// </summary>
    AssignInternal,
    
    /// <summary>
    /// Go to the line number currently on the stack
    /// </summary>
    Goto,
    
    /// <summary>
    /// Load a constant value onto the stack
    /// </summary>
    LoadConstString,

    /// <summary>
    /// Load a constant value onto the stack
    /// </summary>
    LoadConstNum,

    /// <summary>
    /// Load an external var onto the stack
    /// </summary>
    LoadExternal,
    
    /// <summary>
    /// Load an internal var onto the stack
    /// </summary>
    LoadInternal,

    /// <summary>
    /// Do a jump if the value on the stack is false
    /// </summary>
    BranchIfFalse,
    
    /// <summary>
    /// Do an unconditional jump
    /// </summary>
    Branch,

    /// <summary>
    /// Immediately trigger a runtime error
    /// </summary>
    RuntimeError,

    Add,
    Sub,
    Mul,
    Div,
    
    /// <summary>
    /// Divide by a small non-zero integer, stored in the operand
    /// </summary>
    DivSmallInt,
    
    Mod,
    
    /// <summary>
    /// Modulo by a small non-zero integer, stored in the operand
    /// </summary>
    ModSmallInt,
    
    Expo,
    Eq,
    EqSmallInt,
    Neq,
    Gt,
    Gteq,
    Lt,
    Lteq,
    And,
    Or,
    Not,
    Abs,
    Neg,
    Fac,
    Sqrt,
    Cos,
    Sin,
    Tan,
    Acos,
    Asin,
    Atan,
    Pop,
    
    PreIncExternal,
    PreIncInternal,
    PreDecExternal,
    PreDecInternal,

    IncVarExternal,
    IncVarInternal,
    DecVarExternal,
    DecVarInternal,
}