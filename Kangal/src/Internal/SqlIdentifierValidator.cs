using System;
using System.Text.RegularExpressions;

namespace Kangal;

/// <summary>
/// Provides validation and sanitization for SQL identifiers (table names, column names) to prevent SQL injection.
/// </summary>
internal static partial class SqlIdentifierValidator
{
    private const int MaxIdentifierLength = 128; // SQL Server max identifier length
    
    [GeneratedRegex(@"^[\p{L}_][\p{L}\p{N}@$#_]{0,127}$", RegexOptions.Compiled)]
    private static partial Regex ValidIdentifierPattern();

    /// <summary>
    /// Escapes a SQL identifier by wrapping it in square brackets and escaping any existing brackets.
    /// </summary>
    /// <param name="identifier">The identifier to escape (table name or column name)</param>
    /// <returns>Escaped identifier safe for use in SQL queries</returns>
    /// <exception cref="ArgumentException">Thrown when the identifier is null, empty, or exceeds the maximum length</exception>
    /// <remarks>
    /// Examples:
    /// - "Users" → "[Users]"
    /// - "User]Table" → "[User]]Table]"
    /// - "My Table" → "[My Table]"
    /// This prevents SQL injection by ensuring identifiers cannot break out of the bracket context.
    /// </remarks>
    public static string EscapeIdentifier(string identifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        if (identifier.Length > MaxIdentifierLength)
        {
            throw new ArgumentException($"SQL identifier exceeds maximum length of {MaxIdentifierLength} characters.", nameof(identifier));
        }

        // Escape closing brackets by doubling them, then wrap in brackets
        var escaped = identifier.Replace("]", "]]");
        return $"[{escaped}]";
    }

    /// <summary>
    /// Validates if a string is a safe SQL identifier without requiring escaping.
    /// </summary>
    /// <param name="identifier">The identifier to validate</param>
    /// <returns>True if the identifier is valid and safe; otherwise false</returns>
    /// <remarks>
    /// Valid identifiers:
    /// - Start with a letter or underscore
    /// - Contain only letters, digits, @, $, #, or underscore
    /// - Maximum 128 characters
    /// Use this for performance-critical paths where you want to avoid bracket escaping.
    /// </remarks>
    public static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || identifier.Length > MaxIdentifierLength)
        {
            return false;
        }

        return ValidIdentifierPattern().IsMatch(identifier);
    }

    /// <summary>
    /// Validates and escapes a SQL identifier or throws if the identifier is invalid.
    /// </summary>
    /// <param name="identifier">The identifier to validate and escape</param>
    /// <param name="parameterName">The parameter name for exception messages</param>
    /// <returns>Escaped identifier safe for use in SQL queries</returns>
    /// <exception cref="ArgumentException">Thrown when identifier is invalid</exception>
    public static string ValidateAndEscape(string identifier, string parameterName)
    {
        try
        {
            return EscapeIdentifier(identifier);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid SQL identifier: {ex.Message}", parameterName, ex);
        }
    }
}
