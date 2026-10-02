---
name: archive-branch
description: Archive an old Unity version branch of this repository as an annotated archive/<version> tag and delete the branch. Maintainer use only.
---

# Archive a version branch

Only for versions that are far out of support. Supported versions stay as live branches.

A branch is archived as an annotated tag named `archive/<version>` (for example `archive/2019`), which preserves its final state permanently.
The README's "Archived Versions" section describes this for users, so keep the two consistent.

Confirm the branch name with the user before running anything, because the final step deletes the remote branch.

1. Tag the tip of the branch.
2. Push the tag.
3. Delete the remote branch.

```bash
git tag -a archive/2019 origin/2019 -m "Archive 2019 branch"
git push origin archive/2019
git push origin --delete 2019
```

Restoring is the user-facing step documented in the README: `git branch 2019 archive/2019`.
