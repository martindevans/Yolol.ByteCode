using Yolol.Grammar;

namespace Yolol.ByteCode;

public class InternalsMap
    : Dictionary<VariableName, byte>, IReadonlyInternalsMap
{
}

public interface IReadonlyInternalsMap
    : IReadOnlyDictionary<VariableName, byte>
{
}