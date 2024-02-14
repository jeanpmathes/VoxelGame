// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly:
    SuppressMessage(
        "Design",
        "CA1062:Validate arguments of public methods",
        Justification = "Not a public API.",
        Scope = "module")]

[assembly:
    SuppressMessage(
        "Design",
        "CA1033:Interface methods should be callable by child types",
        Justification
            = "Used for interface-based pattern where concrete class implements " +
              "interface that provides methods for another, higher interface.")]
