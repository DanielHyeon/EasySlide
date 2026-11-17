# Git Hooks

This repository stores shared hooks in `.githooks`.

Enable them for a local clone:

```powershell
git config core.hooksPath .githooks
```

The `pre-commit`, `post-commit`, and `pre-push` hooks refresh the local CodeGraph index. If a commit adds new C# files, `pre-commit` runs a full CodeGraph re-index so new symbols are visible before follow-up work.
