# AGENTS.md

## Working Method

* Inspect the relevant existing code before making changes.
* Understand how the affected components are connected before editing.
* Follow existing project patterns unless there is a concrete reason not to.
* Prefer the smallest change that fully solves the requested problem.
* Do not perform unrelated refactors while implementing a task.
* Do not guess about existing APIs, types, configuration, architecture, repository state, or issue requirements. Inspect them.
* For non-trivial tasks, form a short implementation plan before editing.
* If changes relevant to the requested task already exist, inspect and preserve them. Do not reimplement work that is already correctly done.
* Treat the current working tree as potentially meaningful user work. Never discard, overwrite, reset, or stash changes merely to make the repository cleaner.

## Architecture

* Respect existing architectural boundaries and dependency direction.
* Business/domain logic must not depend on infrastructure-specific implementations.
* Prefer extending existing abstractions over creating parallel implementations.
* Before introducing a new package, service, abstraction, or architectural pattern, check whether the project already has an equivalent.
* Prefer existing project conventions over generic best practices when both are valid.
* Architectural changes should be justified by the task, not introduced solely because another pattern is more fashionable or theoretically cleaner.
* Read relevant domain documentation and ADRs before changing architecture.
* If a proposed change conflicts with an ADR, surface the conflict instead of silently overriding the decision.

## Code Changes

* Preserve backward compatibility unless the task explicitly requires a breaking change.
* Match naming, formatting, error handling, testing, and dependency-injection patterns already used nearby.
* Do not silently swallow errors.
* Do not hardcode secrets, credentials, environment-specific URLs, or tokens.
* Keep changes scoped to the requested task or issue.
* Do not modify unrelated files.

## Verification

Before declaring a coding task complete:

1. Build the affected project.
2. Run relevant tests.
3. Add or update tests when behavior changes.
4. Check compiler/type errors.
5. Inspect the resulting diff for unrelated changes.
6. Verify that newly introduced APIs match the installed dependency versions.
7. Report any verification step that could not be completed.
8. If indexed project files changed, run qdrant-code reindex_changes for /workspace once near task completion. Do not reindex after every individual edit or commit.
9. If fresh semantic code search is needed earlier during implementation, run `reindex_changes` before relying on the updated index.

If verification fails, investigate and fix the failure instead of claiming completion.

Do not claim a test, build, command, MCP call, browser check, commit, push, or pull request succeeded unless it was actually executed successfully.

Documentation-only tasks do not require tests that cannot meaningfully exercise the change, but still inspect the diff and run any relevant validation available.

## Git

* Do not commit, push, merge, rebase, or force-push during ordinary coding requests unless explicitly requested.
* An explicit request to `implement issue #X`, `implement #X`, or equivalent wording is an exception: treat it as authorization to complete the full issue-to-pull-request workflow defined below.
* Creating a pull request is part of implementing an issue.
* Merging a pull request is never implied by implementing an issue.
* Never merge a pull request unless explicitly requested.
* Never add secrets or local credentials to Git.
* Never force-push the repository's default branch.

### Implementing GitHub issues

When the user asks to `implement issue #X` or equivalent:

1. Read the existing issue from the configured issue tracker before changing anything.
2. Confirm the issue number, title, requirements, acceptance criteria, and relevant discussion.
3. Do not create a new issue when the requested issue already exists.
4. Inspect the working tree and existing changes.
5. If existing uncommitted changes already implement part or all of the issue, preserve and evaluate them instead of starting over.
6. Determine the repository's actual default branch.
7. Fetch the latest remote state.
8. Create or switch to a dedicated issue branch before committing the implementation.
9. Base new issue work on the latest remote default branch when it is safe to do so without losing existing work.
10. Implement only the scope required by the issue.
11. Build and run relevant tests.
12. Run all other applicable verification checks.
13. Run `qdrant-code` `reindex_changes` for `/workspace` if source files changed.
14. Inspect the final diff for unrelated changes.
15. Commit the completed issue changes with a concise issue-relevant commit message.
16. Push the feature branch.
17. Create a pull request targeting the repository's actual default branch.
18. Use a pull-request title that clearly corresponds to the issue.
19. Include `Closes #X` in the pull-request description when the pull request fully resolves the issue.
20. Report:

    * issue
    * branch
    * commit
    * pull request
    * build result
    * test result
    * reindex result when applicable
    * any remaining concerns

If the implementation was already present in the working tree when the user requested `implement issue #X`, do not redo it. Validate the existing changes, create the appropriate feature branch safely, complete verification, commit, push, and open the pull request.

Do not stop after modifying files when the request was to implement an issue. The task is not complete until the pull request has been created, unless a concrete blocker prevents it.

### Branch and PR workflow

* Before creating a new feature branch, fetch the latest remote state.

* Do not assume local `main`, `master`, or another default branch is current.

* Determine the repository's actual default branch instead of assuming it is `main`.

* Use a concise issue-related branch name, for example:

  `16-update-agent-guidelines`

* Preserve relevant existing uncommitted work when creating an issue branch.

* Never reset or discard existing work merely to recreate the branch from a cleaner state.

* If the remote default branch advanced while relevant uncommitted work exists, preserve the work first and reconcile the branch conservatively.

* When preparing an existing feature branch for a pull request, fetch the latest remote default branch and determine whether the feature branch is behind or conflicts with it.

* If explicitly asked to update an existing feature branch, rebase it onto the latest remote default branch unless the repository follows a different established merge strategy.

* Resolve conflicts conservatively. Preserve both the intended feature behavior and relevant changes from the latest default branch.

* After resolving conflicts, rebuild and rerun relevant tests before pushing.

* If a rebase rewrites feature-branch history and a push is authorized, use `--force-with-lease`, never plain `--force`.

* Never force-push the repository's default branch.

* Never merge a pull request unless explicitly requested.

## Documentation

* Update documentation when public behavior, configuration, setup, architecture, or developer workflows change.
* Keep detailed architecture documentation outside this file and read it when relevant.
* Use the project's existing domain terminology.
* Do not create ADRs merely because a decision exists. Use the project's established ADR conventions and create one only when the decision is architecturally significant and worth preserving.

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

Treat different tools as different sources with different responsibilities.

### Source of truth

* The repository working tree is the final source of truth for current implementation behavior.
* Never assume an index, project-memory entry, documentation page, issue description, or model memory is newer than the working tree.
* After discovering relevant code through another tool, inspect the actual source before editing it.

### Code discovery with qdrant-code

* Use `qdrant-code` for semantic source-code discovery when looking for implementations, related behavior, relevant files, classes, methods, tests, configuration, or architectural relationships by meaning rather than exact text.
* Before relying on `qdrant-code`, check the index status for `/workspace`.
* If the current repository is not indexed, run `index_codebase` for `/workspace`.
* After source files change, use `reindex_changes` before relying on semantic results that need to include those changes.
* Run `reindex_changes` once near task completion when source files changed so the next session receives an up-to-date index.
* Prefer `reindex_changes` over rebuilding the complete index.
* Use a full reindex only when:

  * the repository has not been indexed,
  * the index was cleared,
  * the index is known to be invalid,
  * or incremental synchronization cannot correctly recover it.
* Do not mechanically reindex after every individual edit. Batch related edits.
* Do not assume the code index is newer than the working tree.
* When `qdrant-code` and the working tree disagree, trust the working tree.

### Exact source search

* Use filesystem inspection and `ripgrep` for:

  * exact identifiers
  * exact strings
  * exhaustive matches
  * references to a known symbol
  * final verification of current file contents
* Semantic search and exact search complement each other; neither replaces direct inspection of source files.

### Search strategy

When investigating an unfamiliar part of the repository, prefer this flow when appropriate:

1. Read relevant domain documentation and ADRs.
2. Use `qdrant-code` for semantic discovery when the exact location is unknown.
3. Inspect the returned source files directly.
4. Follow imports, references, callers, tests, and configuration needed to understand the affected behavior.
5. Use `ripgrep` or filesystem search for exact and exhaustive verification.
6. Use durable project knowledge when historical or project-specific context is useful.
7. Verify implementation-sensitive conclusions against the current working tree.

Do not perform broad manual repository scanning when a targeted semantic search can locate the relevant area more efficiently.

### Project knowledge with qdrant

* Use the `qdrant` project-knowledge MCP for durable project context such as:

  * architecture decisions
  * important implementation constraints
  * established project conventions
  * non-obvious component relationships
  * important file locations
  * previous reusable solutions
  * deployment and operational knowledge
* Treat project knowledge as discovery and context rather than authoritative current implementation state.
* Verify implementation-sensitive knowledge against the repository before acting on it.
* Do not use the curated `qdrant` knowledge collection as a replacement for `qdrant-code`.
* Do not store information merely because it was produced during a task.
* Do not store:

  * transient debugging output
  * temporary implementation state
  * raw test output
  * temporary branch state
  * information trivially rediscoverable from source code
* Store durable knowledge only when it is likely to help future sessions and cannot be obtained more reliably from current source or existing documentation.

### External documentation and services

* Use Context7 for current third-party library and framework documentation.
* Use Microsoft Learn for current .NET, ASP.NET Core, Azure, and other Microsoft documentation.
* Use GitHub MCP for remote repository state such as:

  * issues
  * issue discussion
  * pull requests
  * branches
  * commits
  * repository metadata
* Use Playwright MCP when browser interaction or rendered-page behavior must be verified.
* Do not rely on model memory for version-sensitive APIs when an authoritative documentation source is available.
* When external documentation conflicts with the repository's existing behavior, preserve existing project behavior unless the task explicitly requires migration or modernization.

## Skills

Installed agent skills provide specialized engineering workflows and reusable engineering discipline.

* Use an installed model-invoked skill when its description clearly matches the current task.
* Do not reproduce an installed skill's entire procedure inside this file.
* Let skills own their specialized workflow while respecting the repository-specific rules in this file.
* Repository safety, Git, source-of-truth, verification, and issue-to-PR rules in this file still apply when using skills.
* If a skill's generic assumptions conflict with explicit repository rules here, follow the repository rules.
* Do not invoke a user-invoked skill on the user's behalf when the skill is configured to require explicit user invocation.
* When the user explicitly invokes a user-invoked skill such as `/implement`, `/triage`, `/to-spec`, or another installed command, follow that skill's workflow.
* Do not pretend to have invoked a skill when it was not actually invoked.
* Use skills as process guidance, not as evidence about the current repository. Inspect the repository and relevant external sources as required.

### Skill and Git interaction

* The natural-language request `implement issue #X` is governed by the full issue workflow in this `AGENTS.md`.
* It does not require the user to explicitly invoke `/implement`.
* When handling `implement issue #X`, create the appropriate feature branch before the implementation is committed.
* Complete the workflow through push and pull-request creation.
* If the user explicitly invokes `/implement`, remember that repository branch state still matters. Do not assume the skill itself created the correct branch.
* Never allow a generic skill workflow to cause commits directly to the repository's default branch when issue work should live on a feature branch.
* Pull-request creation remains required for natural-language `implement issue #X`, even if an invoked implementation skill stops after committing.

### Skill selection

When relevant installed skills are available:

* Use testing/TDD skills for test-driven feature or bug work when their invocation rules permit it.
* Use diagnosis/debugging skills for difficult bugs or regressions instead of guessing at causes.
* Use code-review skills for structured review when requested or when another invoked workflow requires them.
* Use architecture/design skills for architecture-specific investigation rather than introducing new patterns ad hoc.
* Use research skills when authoritative external technical research is required.
* Use domain-modeling skills when domain terminology or conceptual boundaries need to be actively developed.
* Use writing-for-agents when creating or substantially revising instructions intended for coding agents, including `AGENTS.md`, `CLAUDE.md`, skills, runtime prompts, or similar agent-facing documentation.
* When unsure which explicitly user-invoked Matt Pocock workflow fits, the user may invoke `/ask-matt`.

Do not call heavyweight workflows merely because they exist. Use the smallest appropriate skill or normal repository workflow that fully addresses the task.

## Agent skills

### Issue tracker

GitHub issues. Use the configured GitHub tooling and repository conventions. See `docs/agents/issue-tracker.md`.

### Triage labels

Default five canonical triage roles. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout (`CONTEXT.md` + `docs/adr/`). See `docs/agents/domain.md`.
