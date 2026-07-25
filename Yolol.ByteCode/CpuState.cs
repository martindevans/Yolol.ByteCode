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

    private Value Pop(Span<Value> stack)
    {
        var v = stack[--_stackPointer];
        stack[_stackPointer] = default;
        return v;
    }

    private void Push(Value value, Span<Value> stack)
    {
        if (value.Type == Type.String)
            value = new Value(YString.Trim(value.String, MaxStringLength));
        
        stack[_stackPointer++] = value;
    }

    private void Push(YString value, Span<Value> stack)
    {
        stack[_stackPointer++] = new Value(YString.Trim(value, MaxStringLength));
    }

    private void Push(Number value, Span<Value> stack)
    {
        stack[_stackPointer++] = value;
    }

    private void SetProgramLine(Value input)
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

    private void SetProgramLine(Number input)
    {
        // Handle ranges
        if (input < Number.One)
            input = Number.One;
        if (input > (Number)Program.MaxLineNumber)
            input = (Number)Program.MaxLineNumber;

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

        // Initialise PC to the start of the line
        var lineStart = Program.LineStarts.Span[_programLine];
        _programCounter = lineStart;
            
        // Convert everything to spawns
        var instructions = Program.Instructions.Span;
        var constants = Program.Constants.Span;
        var externals = _externals.Span;
        var internals = _internals.Span;
        var stack = _stack.Span;

        // Get the span of labels for this line. Each line has 255 labels assigned to it, so take that slice.
        var labels = Program.Labels.Slice(_programLine * byte.MaxValue, byte.MaxValue).Span;

        // Clear the execution stack
        _stackPointer = 0;

        try
        {
            // Execute all of the instructions in the line
            while (true)
            {
                var instruction = instructions[_programCounter++];
                switch (instruction.Op)
                {
                    case Op.EndOfLine:
                        SetProgramLine(YololLineNumber + Number.One);
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

                    case Op.LoadConst:
                        Push(constants[instruction.Operand], stack);
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
                        _programCounter = lineStart + labels[instruction.Operand];
                        break;

                    case Op.Add:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                        Push(a + b, stack);
                        break;
                    }

                    case Op.Sub:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                        Push(a - b, stack);
                        break;
                    }

                    case Op.Mul:
                    {
                        var b = Pop(stack);
                        var a = Pop(stack);
                        if (Value.WillMulThrow(a, b))
                        {
                            RuntimeError();
                            return;
                        }
                        
                        Push(Value.UnsafeMultiply(a, b), stack);
                        break;
                    }

                    case Op.Div:
                    {
                        var b = Pop(stack);
                        var a = Pop(stack);
                        if (Value.WillDivThrow(a, b))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeDiv(a, b), stack);
                        break;
                    }

                    case Op.Mod:
                    {
                        var b = Pop(stack);
                        var a = Pop(stack);
                        if (Value.WillModThrow(a, b))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeMod(a, b), stack);
                        break;
                    }

                    case Op.Expo:
                    {
                        var b = Pop(stack);
                        var a = Pop(stack);
                        if (Value.WillExponentThrow(a, b))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeExponent(a, b), stack);
                        break;
                    }

                    case Op.Eq:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);

                        Push(new Value(a == b), stack);
                        break;
                    }

                    case Op.Neq:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                            
                        Push(new Value(a != b), stack);
                        break;
                    }

                    case Op.Gt:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                        
                        Push(new Value(a > b), stack);
                        break;
                    }

                    case Op.Gteq:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);

                        Push(new Value(a >= b), stack);
                        break;
                    }

                    case Op.Lt:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                        
                        Push(new Value(a < b), stack);
                        break;
                    }

                    case Op.Lteq:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);

                        Push(new Value(a <= b), stack);
                        break;
                    }

                    case Op.And:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                        
                        Push(new Value(a & b), stack);
                        break;
                    }

                    case Op.Or:
                    {
                        // Never throws!
                        var b = Pop(stack);
                        var a = Pop(stack);
                        
                        Push(new Value(a | b), stack);
                        break;
                    }

                    case Op.Not:
                    {
                        // Never throws!
                        Push(new Value(!Pop(stack)), stack);
                        break;
                    }

                    case Op.Abs:
                    {
                        var a = Pop(stack);
                        if (Value.WillAbsThrow(a))
                        {
                            RuntimeError();
                            return;
                        }
                        
                        Push(Value.UnsafeAbs(a), stack);
                        break;
                    }

                    case Op.Neg:
                    {
                        var a = Pop(stack);
                        if (Value.WillNegateThrow(a))
                        {
                            RuntimeError();
                            return;
                        }
                        
                        Push(Value.UnsafeNegate(a), stack);
                        break;
                    }

                    case Op.Fac:
                    {
                        var a = Pop(stack);
                        if (Value.WillFactorialThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeFactorial(a), stack);
                        break;
                    }

                    case Op.Sqrt:
                    {
                        var a = Pop(stack);
                        if (Value.WillSqrtThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeSqrt(a), stack);
                        break;
                    }

                    case Op.Cos:
                    {
                        var a = Pop(stack);
                        if (Value.WillCosThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeCos(a), stack);
                        break;
                    }

                    case Op.Sin:
                    {
                        var a = Pop(stack);
                        if (Value.WillSinThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeSin(a), stack);
                        break;
                    }

                    case Op.Tan:
                    {
                        var a = Pop(stack);
                        if (Value.WillTanThrow(a))
                        {
                            RuntimeError();
                            return;
                        }
                        
                        Push(Value.UnsafeTan(a), stack);
                        break;
                    }

                    case Op.Acos:
                    {
                        var a = Pop(stack);
                        if (Value.WillArcCosThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeArcCos(a), stack);
                        break;
                    }

                    case Op.Asin:
                    {
                        var a = Pop(stack);
                        if (Value.WillArcSinThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeArcSin(a), stack);
                        break;
                    }

                    case Op.Atan:
                    {
                        var a = Pop(stack);
                        if (Value.WillAtanThrow(a))
                        {
                            RuntimeError();
                            return;
                        }

                        Push(Value.UnsafeAtan(a), stack);
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
                        throw new InvalidOperationException($"Unknown OpCode: {instruction.Op}");
                }
            }
        }
        catch (ExecutionException)
        {
            // This is just for safety! All exceptions should be avoided by the WillXThrow checks, but if
            // we've missed something it will fall through to here and operate correctly (albeit slowly).
            RuntimeError();
        }
    }
}