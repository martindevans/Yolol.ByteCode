using Yolol.Analysis.TreeVisitor.Inspection;
using Yolol.ByteCode.Instructions;
using Yolol.Execution;
using Yolol.Grammar;
using Yolol.Grammar.AST;

namespace Yolol.ByteCode.Compiler;

public static class CompileExtensions
{
    /// <summary>
    /// Pre populate maps with variables.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="internalVariableMap"></param>
    /// <param name="externalVariableMap"></param>
    private static void Prepare(this Line line, InternalsMap internalVariableMap, ExternalsMap externalVariableMap)
    {
        // Locate all accessed variables
        var stored = new FindAssignedVariables();
        stored.Visit(line);
        var loaded = new FindReadVariables();
        loaded.Visit(line);

        // Populate maps
        foreach (var name in stored.Names.Concat(loaded.Names).Distinct())
        {
            var dict = name.IsExternal ? (Dictionary<VariableName, byte>)externalVariableMap : internalVariableMap;
            if (!dict.TryGetValue(name, out _))
                dict[name] = checked((byte)dict.Count);
        }
    }

    public static CompiledProgram Compile(this Program program, InternalsMap internals, ExternalsMap externals)
    {
        // Populate maps with all variables
        foreach (var line in program.Lines)
            line.Prepare(internals, externals);

        var maxLines = Math.Max(20, program.Lines.Count);
        var output = new List<Instruction>();
        var constants = new List<Value>();
        var lineLabels = new Dictionary<int, int>();
        var lineStarts = new int[program.Lines.Count];

        var lineIdx = 0;
        foreach (var line in program.Lines)
        {
            // Store the index of the first instruction on this line
            lineStarts[lineIdx] = output.Count;

            // Convert line into instructions
            var labels = new Dictionary<Emitter.LabelId, int>();
            var lineOutput = new List<Instruction>();
            new ConvertLineVisitor(lineOutput, constants, internals, externals, 20, labels).Visit(line);

            // Copy output instructions to main output block
            output.AddRange(lineOutput);

            // Store the labels for this line, keyed by their index in the main labels array
            foreach (var (id, idx) in labels)
                lineLabels[lineIdx * byte.MaxValue + id.Id] = idx;

            // Advance to the next line
            lineIdx++;
        }

        // There are 255 labels per line, allocate a large enough array and copy them over
        var labelsArr = new int[lineIdx * byte.MaxValue];
        Array.Fill(labelsArr, -1);
        foreach (var (idx, val) in lineLabels)
            labelsArr[idx] = val;

        return new CompiledProgram(
            output.ToArray(),
            constants.ToArray(),
            maxLines,
            lineStarts,
            labelsArr
        );
    }
}