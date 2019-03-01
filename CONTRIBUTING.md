# Contributing

## Coding style

Indent using 4 spaces (__not__ tabs).

Basically follow [Microsoft guidelines](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/)

## Git workflow

### Commits

Make commits as frequently to not miss code you wrote.

Before creating merge request squash commits into few logical ones. `git rebase -i origin/master` is your friend.

Make commit messages meaningful. ("Uploading new file", "Changes", "Something", ".", etc. are __bad__ examples).

Remove white characters from line endings.

### Branches

__Never__ commit to someone else's branch.

If you can, create branch from master.

If you want to pull from someone else's branch, do it, but remember to create merge request __after__ base branch is merged.

### Merge requests

Your code has to be fast-forwardable. If it is not, `git rebase origin/master` is your friend

Keep code linear. No merge commits. If you have them in your request, `git rebase -i origin/master` is your friend.

If you are not planning to use your branch, merge it marking `Delete source branch`.

### Code review

Do not be affraid to point possible mistakes or flaws even if you are not sure if that is the case. You are probably right.

Do not be affraid to ask about part of the code you do not understand. You are probably not alone.

Review also commit messages and git graph.
