# CLAUDE.md

@AGENTS.md

## GitHub Ticket Workflow

When asked to implement a GitHub issue:

1. Read the issue with `gh issue view <number>`.
2. Ensure the working tree is clean.
3. Update the local main branch.
4. Create a linked development branch with:
   `gh issue develop <number> --checkout --base main`
5. Implement the ticket using the `/implement` workflow.
6. Run all relevant tests and checks.
7. Commit the work to the issue branch.
8. Push the branch to origin.
9. Create a pull request against `main`.
10. Include `Closes #<issue>` in the PR body.
11. Never merge the PR.
12. Never push directly to `main`.

The human maintainer is responsible for review and merge.