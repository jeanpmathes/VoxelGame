﻿// <copyright file="GlobalSupressions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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

[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "All controls are disposed by their parent.", Scope = "module")]
