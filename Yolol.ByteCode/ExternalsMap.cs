using Yolol.Grammar;

namespace Yolol.ByteCode;

public class ExternalsMap
    : Dictionary<VariableName, byte>, IReadonlyExternalsMap
{
}

public interface IReadonlyExternalsMap
    : IReadOnlyDictionary<VariableName, byte>
{
}