# Agent Guidelines

## C# Coding Conventions

- **File Header**: Every source file begins with a license header similar to:
  ```
  // <copyright file="File.cs" company="VoxelGame">
  //     VoxelGame - a voxel-based video game.
  //     Copyright (C) 2025 Jean Patrick Mathes
  //     
  //     This program is free software: you can redistribute it and/or modify
  //     it under the terms of the GNU General Public License as published by
  //     the Free Software Foundation, either version 3 of the License, or
  //     (at your option) any later version.
  //     
  //     This program is distributed in the hope that it will be useful,
  //     but WITHOUT ANY WARRANTY; without even the implied warranty of
  //     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  //     GNU General Public License for more details.
  //     
  //     You should have received a copy of the GNU General Public License
  //     along with this program.  If not, see <https://www.gnu.org/licenses/>.
  // </copyright>
  // <author>jeanpmathes</author>
  ```
- **Namespaces**: Files use file-scoped namespaces (`namespace VoxelGame.Core;`).
- **Type Names**: Use CLR type names (`String`, `Int32`, `Boolean`, etc.) instead of the C# keywords `string`, `int`, `bool`.
- **Indentation and Braces**: Indent with four spaces and place opening braces on a new line (Allman style).
- **Documentation**: Public and internal members should have XML documentation comments (`///`).
- **Logging**: Logging should use a source generator, which means each message needs a `partial` methods decorated with `[LoggerMessage]`.
- **Regions**: `#region`/`#endregion` must be used for logging and implementations of disposable and equatable. Other use is discouraged.
- **Var Usage**: `var` is rarely used; explicit type names are preferred unless the type is obvious.

## C++ Coding Conventions

- **File Header**: As with C#, all files start with the same license header.
- **Includes**: Files include `stdafx.h` first.
- **Header Guards**: Header files use `#pragma once` and rely on `stdafx.h` for
  precompiled headers.
- **Indentation and Braces**: Four‑space indentation with braces on the next line (Allman style).
- **Member Fields**: Private members use the `m_` prefix (`m_width`, `m_logicTimer`).
- **Function Names**: Methods use PascalCase. Free functions inside anonymous namespaces are also PascalCase.
- **Enums**: Strongly typed `enum class` is used with uppercase enumerator names, e.g., `ARROW`, `SIZE_NWSE`.
- **Attributes and Macros**: `[[nodiscard]]` appears on const getters. Helper macros (`NATIVE`, `TRY`, `CATCH`, `DEFINE_ENUM_FLAG_OPERATORS`) are used.
- **Comments**: Multi‑line comments use Doxygen style (`/** ... */`).

## General Notes

- The coding style favors explicitness and clear naming over brevity. As such, abbreviations are avoided unless they are widely recognized.
- Consistency within a file and across the codebase is prioritized.

## Commit Messages

- Start with a short, imperative summary. Example: `Add native allocator`.
- Begin the summary with a capital letter and omit the trailing period.

## Comment Guidelines

- Use `///` XML documentation comments for public and internal C# members.
- Place the summary text on a new indented line inside `<summary>` tags.
- Use `<param>`, `<returns>`, and `<exception>` as appropriate and `<inheritdoc />` for overrides.
- Regular comments explain complex logic and avoid restating obvious code.
- C++ headers employ Doxygen style (`/** ... */`) comments.
