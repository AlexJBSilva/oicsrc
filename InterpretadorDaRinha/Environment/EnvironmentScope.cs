namespace InterpretadorDaRinha.Environment;

using InterpretadorDaRinha.RinhaNodes;

public class EnvironmentScope
{
    public EnvironmentScope()
    {
        this.Variables = new();
    }
    public Dictionary<string, Term> Variables { get; set; }

    public static bool IsReturnValueType(dynamic term)
    {
        return term.GetType() == typeof(Str)
            || term.GetType() == typeof(Int)
            || term.GetType() == typeof(Bool)
            || term.GetType() == typeof(ValueType);
    }
}
