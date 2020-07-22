// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Alternative (Attributes) not an option.", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Not a public API.", Scope = "module")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "It looks cool.", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Logging does not have to be localized.", Scope = "module")]
