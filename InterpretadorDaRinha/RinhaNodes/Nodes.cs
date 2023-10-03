
namespace InterpretadorDaRinha.RinhaNodes;

using System.Text.Json.Serialization;
using InterpretadorDaRinha.CustomJsonConverter;
using InterpretadorDaRinha.Environment;

public class Term
{
    public string Kind { get; set; }
    public Location Location { get; set; }

    public virtual dynamic Interprete(EnvironmentScope scope)
    {
        return this;
    }
}

public class FileAst
{
    public string Kind { get; set; }
    public Location Location { get; set; }
    public string Name { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Expression { get; set; }
}

public class Location
{
    public int Start { get; set; }
    public int End { get; set; }
    public string FileName { get; set; }

    public override string ToString() => $"<{Start},{End}>";
}

public class Parameter
{
    public string Kind { get; set; }
    public Location Location { get; set; }
    public string Text { get; set; }

    public override string ToString() => Text;
}

public class Var : Term
{
    public string Text { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        if (scope.Variables.TryGetValue(Text, out Term term))
        {
            return term.Interprete(scope);
        }
        throw new Exception($"Variable name '{Text}' not found in scope.");
    }
    public override string ToString() => $"Var: {Text} @ {Location}";
}

public class Function : Term
{
    public Parameter[] Parameters { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Value { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        return Value.Interprete(scope);
    }
    public override string ToString() =>
        $"Function: {Value} [{string.Join(',', (object[])Parameters)}] @ {Location}";
}

public class Call : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term Callee { get; set; }
    [JsonConverter(typeof(ArrayConverter))]
    public Term[] Arguments { get; set; }
    public override dynamic Interprete(EnvironmentScope scope)
    {
        if ((Callee.GetType() != typeof(Var)) && (Callee.GetType() != typeof(Function)))
            return Callee;

        if (!scope.Variables.TryGetValue(((Var)Callee).Text, out Term term))
        {
            throw new Exception($"Variable name '{((Var)Callee).Text}' not found in scope.");
        }

        Function function = (Function)term;

        if (function.Parameters.Length != Arguments.Length)
        {
            throw new ArgumentException(
                $"Invalid number of arguments. Expected {function.Parameters.Length}, but received {Arguments.Length}");
        }

        EnvironmentScope functionScope = new()
        {
            Variables = new(scope.Variables)
        };

        for (int i = 0; i < Arguments.Length; i++)
        {
            var arg = Arguments[i].Interprete(scope);
            functionScope.Variables[function.Parameters[i].Text] = arg;
        }

        return function.Interprete(functionScope);
    }
}

public class Let : Term
{
    public Parameter Name { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Value { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Next { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        if (Value.GetType() == typeof(Function))
        {
            scope.Variables[Name.Text] = Value;
            return Next.Interprete(scope);
        }

        scope.Variables[Name.Text] = Value.Interprete(scope);
        return Next.Interprete(scope);
    }

    public override string ToString() => $"Let: {Name} | {Value} | {Next} @ {Location}";
}

public class Str : Term
{
    public string Value { get; set; }

    public override string ToString() => Value;

    public static Str operator +(Str a, Str b)
    {
        return new Str
        {
            Value = a.Value + b.Value
        };
    }

    public static Str operator +(Str a, Int b)
    {
        return new Str
        {
            Value = a.Value + b.Value
        };
    }

    public static bool operator ==(Str a, Str b)
        => a.Value == b.Value;

    public static bool operator !=(Str a, Str b)
        => a.Value != b.Value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        return this.Value.Equals(((Str)obj).Value);
    }
    public override int GetHashCode() => Value.GetHashCode();
}

public class Int : Term
{
    public int Value { get; set; }

    public override string ToString() => Value.ToString();

    public static Int operator +(Int a) => a;
    public static Int operator -(Int a)
    {
        return new Int
        {
            Value = -a.Value
        };
    }

    public static Int operator +(Int a, Int b)
    {
        return new Int
        {
            Value = a.Value + b.Value
        };
    }

    public static Str operator +(Int a, Str b)
    {
        return new Str
        {
            Value = a.Value + b.Value
        };
    }

    public static Int operator -(Int a, Int b)
        => a + (-b);

    public static Int operator *(Int a, Int b)
    {
        return new Int { Value = a.Value * b.Value };
    }

    public static Int operator /(Int a, Int b)
    {
        if (b.Value == 0)
        {
            throw new DivideByZeroException();
        }
        return new Int { Value = a.Value / b.Value };
    }

    public static Int operator %(Int a, Int b)
    {
        if (b.Value == 0)
        {
            throw new DivideByZeroException();
        }
        return new Int { Value = a.Value % b.Value };
    }

    public static bool operator ==(Int a, Int b)
        => a.Value == b.Value;

    public static bool operator !=(Int a, Int b)
        => a.Value != b.Value;

    public static bool operator <(Int a, Int b)
        => a.Value < b.Value;

    public static bool operator >(Int a, Int b)
        => a.Value > b.Value;
    public static bool operator <=(Int a, Int b)
        => a.Value <= b.Value;

    public static bool operator >=(Int a, Int b)
        => a.Value >= b.Value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        return this.Value.Equals(((Int)obj).Value);
    }

    public override int GetHashCode() => Value.GetHashCode();

}

public enum BinaryOp
{
    Add,
    Sub,
    Mul,
    Div,
    Rem,
    Eq,
    Neq,
    Lt,
    Gt,
    Lte,
    Gte,
    And,
    Or,
}

public class Bool : Term
{
    public bool Value { get; set; }

    public override string ToString() => Value.ToString().ToLower();

    public static implicit operator bool(Bool b) => b.Value;
    public static explicit operator Bool(bool b) => new() { Value = b };

    public static bool operator ==(Bool a, Bool b)
        => a.Value == b.Value;

    public static bool operator !=(Bool a, Bool b)
        => a.Value != b.Value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        return this.Value.Equals(((Bool)obj).Value);
    }
    public override int GetHashCode() => Value.GetHashCode();
}

public class If : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term Condition { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Then { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Otherwise { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        if (Condition.Interprete(scope))
            return Then.Interprete(scope);

        return Otherwise.Interprete(scope);
    }
    public override string ToString() => $"If: {Condition} | {Then} | {Otherwise} @ {Location}";
}

public class Binary : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term Lhs { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BinaryOp Op { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Rhs { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        return Op switch
        {
            BinaryOp.Add => Binary.Add(Lhs, Rhs, scope),
            BinaryOp.Sub => Binary.Sub(Lhs, Rhs, scope),
            BinaryOp.Mul => Binary.Mul(Lhs, Rhs, scope),
            BinaryOp.Div => Binary.Div(Lhs, Rhs, scope),
            BinaryOp.Rem => Binary.Rem(Lhs, Rhs, scope),
            BinaryOp.Eq => Binary.Eq(Lhs, Rhs, scope),
            BinaryOp.Neq => Binary.Neq(Lhs, Rhs, scope),
            BinaryOp.Lt => Binary.Lt(Lhs, Rhs, scope),
            BinaryOp.Gt => Binary.Gt(Lhs, Rhs, scope),
            BinaryOp.Lte => Binary.Lte(Lhs, Rhs, scope),
            BinaryOp.Gte => Binary.Gte(Lhs, Rhs, scope),
            BinaryOp.And => Binary.And(Lhs, Rhs, scope),
            BinaryOp.Or => Binary.Or(Lhs, Rhs, scope),
            _ => throw new NotImplementedException(),
        };
    }

    private static dynamic Add(Term lhs, Term rhs, EnvironmentScope scope) =>
        lhs.Interprete(scope) + rhs.Interprete(scope);

    private static dynamic Sub(Term lhs, Term rhs, EnvironmentScope scope) =>
        lhs.Interprete(scope) - rhs.Interprete(scope);

    private static dynamic Mul(Term lhs, Term rhs, EnvironmentScope scope) =>
        lhs.Interprete(scope) * rhs.Interprete(scope);

    private static dynamic Div(Term lhs, Term rhs, EnvironmentScope scope) =>
        lhs.Interprete(scope) / rhs.Interprete(scope);
 
    private static dynamic Rem(Term lhs, Term rhs, EnvironmentScope scope) =>
        lhs.Interprete(scope) % rhs.Interprete(scope);
 
    private static dynamic Eq(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) == rhs.Interprete(scope) };
 
    private static dynamic Neq(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) != rhs.Interprete(scope) };
 
    private static dynamic Lt(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) < rhs.Interprete(scope) };

    private static dynamic Gt(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) > rhs.Interprete(scope) };

    private static dynamic Lte(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) <= rhs.Interprete(scope) };

    private static dynamic Gte(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) >= rhs.Interprete(scope) };

    private static dynamic And(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) && rhs.Interprete(scope) };

    private static dynamic Or(Term lhs, Term rhs, EnvironmentScope scope) =>
        new Bool { Value = lhs.Interprete(scope) || rhs.Interprete(scope) };

    public override string ToString() => $"Binary: {Lhs} {Op} {Rhs}";
}

public class TupleRinha : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term First { get; set; }
    [JsonConverter(typeof(TermConverter))]
    public Term Second { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        if (!EnvironmentScope.IsReturnValueType(First))
            First = First.Interprete(scope);

        if (!EnvironmentScope.IsReturnValueType(Second))
            Second = Second.Interprete(scope);

        return this;
    }
    public override string ToString() => $"({First},{Second})";
}

public class First : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term Value { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        if ((Value.GetType() != typeof(TupleRinha)) && (Value.GetType() != typeof(Var))) // TODO: Validate beforehand if 'Var' is Tuple and raise exception.
        {
            throw new ArgumentException($"Expected '{typeof(TupleRinha)}', but '{Value.GetType()}' received.", nameof(Value));
        }

        return Value.Interprete(scope).First;
    }
}

public class Second : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term Value { get; set; }
 
    public override dynamic Interprete(EnvironmentScope scope)
    {
        if ((Value.GetType() != typeof(TupleRinha)) && (Value.GetType() != typeof(Var))) // TODO: Validate beforehand if 'Var' is Tuple and raise exception.
        {
            throw new ArgumentException($"Expected '{typeof(TupleRinha)}', but '{Value.GetType()}' received.", nameof(Value));
        }
        return Value.Interprete(scope).Second;
    }
}

public class Print : Term
{
    [JsonConverter(typeof(TermConverter))]
    public Term Value { get; set; }

    public override dynamic Interprete(EnvironmentScope scope)
    {
        if ((!EnvironmentScope.IsReturnValueType(Value)) && (Value.GetType() != typeof(TupleRinha)))
        {
            Value = Value.Interprete(scope);
        }

        Console.WriteLine(Value);
        return Value;
    }
}
