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
- Prefer existing project conventions over generic best practices when both are valid.
- Architectural changes should be justified by the task, not introduced solely because another pattern is more fashionable or theoretically cleaner.


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
6. Verify that newly introduced APIs match the installed dependency versions.
7. Report any verification step that could not be completed.

If verification fails, investigate and fix the failure instead of claiming completion.
Do not claim a test, build, command, or browser check succeeded unless it was actually run successfully.

## Git

- Do not commit, push, merge, rebase, or force-push unless explicitly requested.
- Do not modify unrelated files.
- Never add secrets or local credentials to Git.

### Branch and PR workflow

- Before creating a new feature branch, fetch the latest remote state and base the branch on the latest remote default branch.
- Do not assume the local `main` or default branch is up to date.
- Determine the repository's actual default branch when needed instead of assuming it is `main`.
- When preparing an existing feature branch for a pull request, fetch the latest remote default branch and check whether the feature branch is behind or conflicts with it.
- If explicitly asked to update the feature branch, rebase it onto the latest remote default branch unless the repository already follows a different merge strategy.
- Resolve conflicts conservatively. Preserve both the intended feature behavior and relevant changes from the latest default branch.
- After resolving conflicts, rebuild and rerun the relevant tests before pushing.
- If a rebase rewrites feature-branch history and a push is explicitly requested, use `--force-with-lease`, never plain `--force`.
- Never force-push the repository's default branch.
- Do not merge a pull request unless explicitly requested.

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

## Tool and Knowledge Routing

- Prefer repository source code as the source of truth for existing project behavior.
- Use project documentation and ADRs for architectural intent and past decisions.
- Use Qdrant project knowledge for discovery and context, but verify important implementation details against the current source code.
- Use Context7 for current third-party library and framework documentation.
- Use Microsoft Learn for current .NET, ASP.NET Core, Azure, and other Microsoft documentation.
- Use GitHub MCP for remote repository state such as issues, pull requests, branches, and repository metadata.
- Use Playwright MCP when browser interaction or rendered-page behavior must be verified.
- Do not rely on model memory for version-sensitive APIs when an authoritative documentation source is available.
- When documentation conflicts with the repository, preserve existing project behavior unless the task explicitly requires migration or modernization.

## Agent skills

### Issue tracker

GitHub issues (uses `gh` CLI). See `docs/agents/issue-tracker.md`.

### Triage labels

Default five canonical triage roles. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout (`CONTEXT.md` + `docs/adr/`). See `docs/agents/domain.md`.