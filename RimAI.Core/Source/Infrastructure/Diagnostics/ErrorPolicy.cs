using System;
using System.Collections.Generic;
using Verse;

namespace RimAI.Core.Source.Infrastructure.Diagnostics
{
    internal enum ErrorSeverity
    {
        Debug,
        Warn,
        Error,
    }

    internal static class ModuleLogPrefixes
    {
        private static readonly Dictionary<string, string> PrefixByModule = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Boot"] = "[RimAI.Core][BOOT]",
            ["History"] = "[RimAI.Core][HISTORY]",
            ["Prompting"] = "[RimAI.Core][PROMPT]",
            ["Orchestration"] = "[RimAI.Core][ORCH]",
            ["Server"] = "[RimAI.Core][SERVER]",
            ["Stage"] = "[RimAI.Core][STAGE]",
            ["Persistence"] = "[RimAI.Core][PERSIST]",
            ["Tooling"] = "[RimAI.Core][TOOLS]",
            ["World"] = "[RimAI.Core][WORLD]",
            ["UI"] = "[RimAI.Core][UI]",
            ["General"] = "[RimAI.Core][CORE]",
        };

        public static string Resolve(string module)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                return PrefixByModule["General"];
            }

            return PrefixByModule.TryGetValue(module, out var prefix)
                ? prefix
                : PrefixByModule["General"];
        }
    }

    internal static class ErrorPolicy
    {
        public static bool IsRetryable(Exception ex)
            => ex is TimeoutException || ex is OperationCanceledException;

        public static bool IsIgnorable(Exception ex)
            => ex is NullReferenceException || ex is InvalidOperationException || ex is ArgumentException;

        public static void Retryable(Exception ex, string module, string operation, string entityOrConvKey = null)
            => Log(ex, module, operation, entityOrConvKey, ErrorSeverity.Debug, "retryable");

        public static void Ignore(Exception ex, string module, string operation, string entityOrConvKey = null)
            => Log(ex, module, operation, entityOrConvKey, ErrorSeverity.Warn, "ignorable");

        public static void Fatal(Exception ex, string module, string operation, string entityOrConvKey = null)
            => Log(ex, module, operation, entityOrConvKey, ErrorSeverity.Error, "fatal");

        private static void Log(Exception ex, string module, string operation, string entityOrConvKey, ErrorSeverity severity, string policy)
        {
            var prefix = ModuleLogPrefixes.Resolve(module);
            var op = string.IsNullOrWhiteSpace(operation) ? "unknown_op" : operation;
            var entity = string.IsNullOrWhiteSpace(entityOrConvKey) ? "n/a" : entityOrConvKey;
            var message = $"{prefix}[{severity.ToString().ToUpperInvariant()}][{policy}] op={op} key={entity} ex={ex.GetType().Name}: {ex.Message}";

            try
            {
                switch (severity)
                {
                    case ErrorSeverity.Debug:
                        Log.Message(message);
                        break;
                    case ErrorSeverity.Warn:
                        Log.Warning(message);
                        break;
                    default:
                        Log.Error(message);
                        break;
                }
            }
            catch
            {
                // Avoid recursive logging failures.
            }
        }
    }
}
