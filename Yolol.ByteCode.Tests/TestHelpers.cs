using Yolol.ByteCode.Compiler;
using Yolol.Execution;
using Yolol.Grammar;
using Yolol.Grammar.AST;

namespace Yolol.ByteCode.Tests;

public static class TestHelpers
{
    public static Program Parse(params string[] lines)
    {
        var result = Parser.ParseProgram(string.Join("\n", lines));
        if (!result.IsOk)
            throw new ArgumentException($"Cannot parse program:\n{result.Err}");

        return result.Ok;
    }

    public static IMachineState Test(string line)
    {
        var internals = new InternalsMap();
        var externals = new ExternalsMap();
        
        var ast = Parse(line);
        var compiled = CompileExtensions.Compile(ast, internals, externals);

        var i = new Value[internals.Count];
        Array.Fill(i, new Value((Number)0));
        var e = new Value[externals.Count];
        Array.Fill(e, new Value((Number)0));

        var cpu = new CpuState(compiled, i, e, new Value[1024]);

        cpu.Execute();

        return new EasyMachineState(i, e, internals, externals, (int)cpu.YololLineNumber);
    }

    public static IMachineState Test(string[] lines, int iterations)
    {
        var prog = Parse(lines);
        return Test(prog, iterations);
    }

    public static IMachineState Test(Program program, int iterations)
    {
        var internals = new InternalsMap();
        var externals = new ExternalsMap();
        
        var compiled = program.Compile(internals, externals);

        var doneIndex = -1;
        if (internals.TryGetValue(new VariableName("done"), out var di))
            doneIndex = di;
        
        var i = new Value[internals.Count];
        Array.Fill(i, new Value((Number)0));

        var e = new Value[externals.Count];
        Array.Fill(e, new Value((Number)0));

        var cpu = new CpuState(compiled, i, e, new Value[1024]);
        for (var j = 0; j < iterations; j++)
        {
            cpu.Execute();

            var done = doneIndex < 0 ? Number.Zero : i[doneIndex];
            if (done.ToBool())
                break;
        }

        return new EasyMachineState(i, e, internals, externals, (int)cpu.YololLineNumber);
    }

    public interface IMachineState
    {
        Value GetVariable(string v);

        int ProgramCounter { get; }
    }

    public class EasyMachineState
        : IMachineState
    {
        public Value[] Internals;
        public Value[] Externals;

        public InternalsMap InternalMap;
        public ExternalsMap ExternalMap;

        public int ProgramCounter { get; }

        public EasyMachineState(Value[] i, Value[] e, InternalsMap internals, ExternalsMap externals, int pc)
        {
            Internals = i;
            Externals = e;
            InternalMap = internals;
            ExternalMap = externals;
            ProgramCounter = pc;
        }

        public Value GetVariable(string v)
        {
            v = v.ToLowerInvariant();
            var n = new VariableName(v);

            if (n.IsExternal)
                return Externals[ExternalMap[n]];
            else
                return Internals[InternalMap[n]];
        }
    }
}