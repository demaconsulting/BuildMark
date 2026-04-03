# Utilities Subsystem

## Overview

The Utilities subsystem provides shared helper classes used across the BuildMark
system. Currently it contains a single unit, `PathHelpers`, which provides safe
path combination with traversal prevention.

The subsystem has no dependencies on other BuildMark subsystems.

## Units

| Unit          | File                       | Responsibility                              |
|---------------|----------------------------|---------------------------------------------|
| `PathHelpers` | `Utilities/PathHelpers.cs` | Safe path combination with traversal checks |

## Interfaces

`PathHelpers` exposes the following static method:

| Member                            | Kind   | Description                                     |
|-----------------------------------|--------|-------------------------------------------------|
| `SafePathCombine(base, relative)` | Method | Safely combine a base path with a relative path |

## Interactions

`PathHelpers` has no dependencies on other BuildMark subsystems. It is used by
any unit that needs to combine file paths safely.
