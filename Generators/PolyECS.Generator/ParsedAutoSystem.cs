using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace PolyECS.Generator;

/// <summary>
/// Helper wrapper for a AutoSystem with support for reporting diagnostics for parse failures
/// </summary>
internal struct ParsedAutoSystem
{
    public static ParsedAutoSystem Empty()
    {
        return new ParsedAutoSystem();
    }

    public static ParsedAutoSystem Valid(AutoSystemBuilder builder)
    {
        return new ParsedAutoSystem
        {
            Value = builder
        };
    }

    public static ParsedAutoSystem Err(Diagnostic err)
    {
        var parsed = new ParsedAutoSystem();
        parsed.Diagnostics.Add(err);
        return parsed;
    }


    public AutoSystemBuilder? Value = null;
    public List<Diagnostic> Diagnostics = new ();

    public ParsedAutoSystem() { }
}