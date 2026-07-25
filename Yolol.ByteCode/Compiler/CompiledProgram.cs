using Yolol.ByteCode.Instructions;
using Yolol.Execution;

namespace Yolol.ByteCode.Compiler;

public struct CompiledProgram
{
    public readonly ReadOnlyMemory<Instruction> Instructions;
    public readonly ReadOnlyMemory<Value> Constants;
    public readonly ReadOnlyMemory<int> LineStarts;
    public readonly ReadOnlyMemory<int> Labels;

    public readonly int MaxLineNumber;

    public CompiledProgram(ReadOnlyMemory<Instruction> instructions, ReadOnlyMemory<Value> constants, int maxLineNumber, ReadOnlyMemory<int> lineStarts, ReadOnlyMemory<int> labels)
    {
        Instructions = instructions;
        Constants = constants;
        MaxLineNumber = maxLineNumber;
        LineStarts = lineStarts;
        Labels = labels;
    }
}