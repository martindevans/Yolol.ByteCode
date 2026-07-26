using Yolol.ByteCode.Compiler;
using Yolol.ByteCode.Instructions;
using Yolol.Execution;
using Type = Yolol.Execution.Type;

namespace Yolol.ByteCode;

public struct CpuState
{
    private int _programLine;
    public Number YololLineNumber
    {
        get => (Number)_programLine + Number.One;
        set => _programLine = (int)value - 1;
    }

    private int _programCounter;
        
    public CompiledProgram Program;
        
    private readonly Memory<Value> _internals;
    private readonly Memory<Value> _externals;

    private int _stackPointer;
    private readonly Memory<Value> _stack;

    public int MaxStringLength { get; }

    public CpuState(CompiledProgram program, Memory<Value> internals, Memory<Value> externals, Memory<Value> stack, int maxStringLength = 1024)
    {
        Program = program;
        _internals = internals;
        _externals = externals;
        _stack = stack;
        MaxStringLength = maxStringLength;
    }

    #region stack
    private ref Value Peek(Span<Value> stack)
    {
        return ref stack[_stackPointer - 1];
    }

    private ref Value Pop(Span<Value> stack)
    {
        return ref stack[--_stackPointer];
    }

    private void Push(in Value value, Span<Value> stack)
    {
        if (value.Type == Type.String)
            Push(value.String, stack);
        else
            stack[_stackPointer++] = value;
    }

    private void Push(in YString value, Span<Value> stack)
    {
        stack[_stackPointer++] = new Value(YString.Trim(value, MaxStringLength));
    }

    private void Push(in Number value, Span<Value> stack)
    {
        stack[_stackPointer++] = value;
    }
    #endregion

    private void SetProgramLine(in Value input)
    {
        // Convert to number
        Number destNum;
        if (input.Type != Type.Number)
            destNum = YololLineNumber + Number.One;
        else
            destNum = input.Number;

        // Do the actual set
        SetProgramLine(destNum);
    }

    private void SetProgramLine(in Number input)
    {
        // Handle ranges
        if (input < Number.One)
        {
            _programLine = 0;
            return;
        }
        if (input > (Number)Program.MaxLineNumber)
        {
            _programLine = Program.MaxLineNumber - 1;
            return;
        }

        // Convert from Yolol line number (1 based) to actual number (0 based)
        _programLine = (int)input - 1;
    }

    private void RuntimeError()
    {
        SetProgramLine(YololLineNumber + Number.One);
    }
    
    /// <summary>
    /// Execute one single line
    /// </summary>
    public void Execute()
    {
        // Treat an empty line as a runtime error (i.e. drop through to the next line)
        if (_programLine >= Program.LineStarts.Length)
        {
            RuntimeError();
            return;
        }

        try
        {
            ExecuteInnerLoop();
        }
        catch (ExecutionException)
        {
            // This is just for safety! All exceptions should be avoided by the WillXThrow checks, but if
            // we've missed something it will fall through to here and operate correctly (albeit slowly).
            RuntimeError();
        }
    }

    private void ExecuteInnerLoop()
    {
        // Initialise PC to the start of the line
        var lineStart = Program.LineStarts.Span[_programLine];
        _programCounter = lineStart;

        // Convert everything to spawns
        var instructions = Program.Instructions.Span;
        var constants = Program.Constants.Span;
        var externals = _externals.Span;
        var internals = _internals.Span;
        var stack = _stack.Span;
        var labels = Program.Labels.Slice(_programLine * byte.MaxValue, byte.MaxValue).Span;

        // Clear the execution stack
        _stackPointer = 0;

        // Execute all of the instructions in the line
        while (true)
        {
            var instruction = instructions[_programCounter++];
            switch (instruction.Op)
            {
                case Op.EndOfLine:
                    SetProgramLine(YololLineNumber + Number.One);
                    return;

                case Op.RuntimeError:
                    RuntimeError();
                    return;

                case Op.AssignExternal:
                    externals[instruction.Operand] = Pop(stack);
                    break;

                case Op.AssignInternal:
                    internals[instruction.Operand] = Pop(stack);
                    break;

                case Op.Goto:
                    SetProgramLine(Pop(stack));
                    return;

                case Op.LoadConstString:
                    Push(constants[instruction.Operand].String, stack);
                    break;

                case Op.LoadConstNum:
                    Push(constants[instruction.Operand].Number, stack);
                    break;

                case Op.LoadExternal:
                    Push(externals[instruction.Operand], stack);
                    break;

                case Op.LoadInternal:
                    Push(internals[instruction.Operand], stack);
                    break;

                case Op.BranchIfFalse:
                {
                    var value = Pop(stack);
                    if (!value.ToBool())
                        _programCounter = lineStart + labels[instruction.Operand];

                    break;
                }

                case Op.Branch:
                {
                    _programCounter = lineStart + labels[instruction.Operand];
                    break;
                }

                case Op.Add:
                {
                    // Never throws!
                    // This is not using the peek trick, we need to push to get the trimming behaviour
                    ref var b = ref Pop(stack);
                    ref var a = ref Pop(stack);
                    Push(a + b, stack);
                    break;
                }

                case Op.Sub:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);
                    a = a - b;
                    break;
                }

                case Op.Mul:
                {
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);
                    if (Value.WillMulThrow(a, b))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeMultiply(a, b);
                    break;
                }

                case Op.Div:
                {
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);
                    if (Value.WillDivThrow(a, b))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeDiv(a, b);
                    break;
                }

                case Op.Mod:
                {
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);
                    if (Value.WillModThrow(a, b))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeMod(a, b);
                    break;
                }

                case Op.Expo:
                {
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);
                    if (Value.WillExponentThrow(a, b))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeExponent(a, b);
                    break;
                }

                case Op.Eq:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a == b);
                    break;
                }

                case Op.Neq:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a != b);
                    break;
                }

                case Op.Gt:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);
                        
                    a = new Value(a > b);
                    break;
                }

                case Op.Gteq:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a >= b);
                    break;
                }

                case Op.Lt:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a < b);
                    break;
                }

                case Op.Lteq:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a <= b);
                    break;
                }

                case Op.And:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a & b);
                    break;
                }

                case Op.Or:
                {
                    // Never throws!
                    ref var b = ref Pop(stack);
                    ref var a = ref Peek(stack);

                    a = new Value(a | b);
                    break;
                }

                case Op.Not:
                {
                    // Never throws!
                    ref var a = ref Peek(stack);
                    a = new Value(!a);
                    break;
                }

                case Op.Abs:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillAbsThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeAbs(a);
                    break;
                }

                case Op.Neg:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillNegateThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeNegate(a);
                    break;
                }

                case Op.Fac:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillFactorialThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeFactorial(a);
                    break;
                }

                case Op.Sqrt:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillSqrtThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeSqrt(a);
                    break;
                }

                case Op.Cos:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillCosThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeCos(a);
                    break;
                }

                case Op.Sin:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillSinThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeSin(a);
                    break;
                }

                case Op.Tan:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillTanThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeTan(a);
                    break;
                }

                case Op.Acos:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillArcCosThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeArcCos(a);
                    break;
                }

                case Op.Asin:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillArcSinThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeArcSin(a);
                    break;
                }

                case Op.Atan:
                {
                    ref var a = ref Peek(stack);
                    if (Value.WillAtanThrow(a))
                    {
                        Pop(stack);
                        RuntimeError();
                        return;
                    }

                    a = Value.UnsafeAtan(a);
                    break;
                }

                case Op.Pop:
                {
                    Pop(stack);
                    break;
                }

                case Op.PreIncExternal:
                {
                    var result = externals[instruction.Operand];
                    result++;
                    externals[instruction.Operand] = result;
                    Push(result, stack);
                    break;
                }

                case Op.PreIncInternal:
                {
                    var result = internals[instruction.Operand];
                    result++;
                    internals[instruction.Operand] = result;
                    Push(result, stack);
                    break;
                }

                case Op.PreDecExternal:
                {
                    var result = externals[instruction.Operand];
                    if (Value.WillDecThrow(result))
                    {
                        RuntimeError();
                        return;
                    }

                    result--;
                    externals[instruction.Operand] = result;
                    Push(result, stack);
                    break;
                }

                case Op.PreDecInternal:
                {
                    var result = internals[instruction.Operand];
                    if (Value.WillDecThrow(result))
                    {
                        RuntimeError();
                        return;
                    }

                    result--;
                    internals[instruction.Operand] = result;
                    Push(result, stack);
                    break;
                }

                default:
                    ThrowUnknownOpcode(instruction);
                    return;
            }
        }
    }

    private void ThrowUnknownOpcode(in Instruction instruction)
    {
        throw new InvalidOperationException($"Unknown OpCode: {instruction.Op}");
    }
}