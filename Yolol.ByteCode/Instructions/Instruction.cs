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

    public static Instruction LoadConst(byte cid)
    {
        return new Instruction { Op = Op.LoadConst, Operand = cid };
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

    public static Instruction Modulo()
    {
        return new Instruction { Op = Op.Mod, Operand = 0 };
    }

    public static Instruction Exponent()
    {
        return new Instruction { Op = Op.Expo, Operand = 0 };
    }

    public static Instruction EqualTo()
    {
        return new Instruction { Op = Op.Eq, Operand = 0 };
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
}

public enum Op
    : byte
{
    /// <summary>
    /// Marks the end of a line. Causes the interpreter to break execution.
    /// </summary>
    EndOfLine = 0,

    /// <summary>
    /// Take value from execution stack and store to an external variable
    /// </summary>
    AssignExternal = 1,
    
    /// <summary>
    /// Take value from execution stack and store to an internal variable
    /// </summary>
    AssignInternal = 2,
    
    /// <summary>
    /// Go to the line number currently on the stack
    /// </summary>
    Goto = 3,
    
    /// <summary>
    /// Load a constant value onto the stack
    /// </summary>
    LoadConst = 4,
    
    /// <summary>
    /// Load an external var onto the stack
    /// </summary>
    LoadExternal = 5,
    
    /// <summary>
    /// Load an internal var onto the stack
    /// </summary>
    LoadInternal = 6,

    /// <summary>
    /// Do a jump if the value on the stack is false
    /// </summary>
    BranchIfFalse = 7,
    
    /// <summary>
    /// Do an unconditional jump
    /// </summary>
    Branch = 8,
    
    Add = 9,
    Sub = 10,
    Mul = 11,
    Div = 12,
    Mod = 13,
    Expo = 14,
    Eq = 15,
    Neq = 16,
    Gt = 17,
    Gteq = 18,
    Lt = 19,
    Lteq = 20,
    And = 21,
    Or = 22,
    Not = 23,
    Abs = 24,
    Neg = 25,
    Fac = 26,
    Sqrt = 27,
    Cos = 28,
    Sin = 29,
    Tan = 30,
    Acos = 31,
    Asin = 32,
    Atan = 33,
    Pop = 34,
    PreIncExternal = 35,
    PreIncInternal = 36,
    PreDecExternal = 37,
    PreDecInternal = 38,
}