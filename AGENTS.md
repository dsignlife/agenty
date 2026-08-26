# AGENTS.md

## Working Method

- Inspect the relevant existing code before making changes.
- Understand how the affected components are connected before editing.
- Follow existing project patterns unless there is a concrete reason not to.
- Prefer the smallest change that fully solves the requested problem.
- Do not perform unrelated refactors while implementing a task.
- Do not guess about existing APIs, types, configuration, or architecture. Inspect them.
- For non-trivial tasks, form a short implementation plan before editing.

## Architecture

- Respect existing architectural boundaries and dependency direction.
- Business/domain logic must not depend on infrastructure-specific implementations.
- Prefer extending existing abstractions over creating parallel implementations.
- Before introducing a new package, service, abstraction, or architectural pattern, check whether the project already has an equivalent.

## Code Changes

- Preserve backward compatibility unless the task explicitly requires a breaking change.
- Match naming, formatting, error handling, and dependency-injection patterns already used nearby.
- Do not silently swallow errors.
- Do not hardcode secrets, credentials, environment-specific URLs, or tokens.

## Verification

Before declaring a coding task complete:

1. Build the affected project.
2. Run relevant tests.
3. Add or update tests when behavior changes.
4. Check compiler/type errors.
5. Inspect the resulting diff for unrelated changes.

If verification fails, investigate and fix the failure instead of claiming completion.

## Git

- Do not commit, push, merge, rebase, or force-push unless explicitly requested.
- Do not modify unrelated files.
- Never add secrets or local credentials to Git.

## Documentation

- Update documentation when public behavior, configuration, setup, or architecture changes.
- Keep detailed architecture documentation outside this file and read it when relevant.

## Development Commands

Build:
`dotnet build`

All tests:
`dotnet test`

Single test project:
`dotnet test tests/MyProject.UnitTests`

Format:
`dotnet format`

Docker:
`docker compose up --build`