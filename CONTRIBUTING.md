# How to contribute efficiently

Sections covered in this file:

* [Reporting bugs or proposing features](#reporting-bugs-or-proposing-features)
* [Contributing pull requests](#contributing-pull-requests)

**Please read the first section before reporting a bug!**

## Reporting bugs or proposing features

The golden rule is to **always open *one* issue for *one* bug**. If you notice
several bugs and want to report them, make sure to create one new issue for
each of them.

If you are reporting a new issue, you will make our life much simpler (and the
fix come much sooner) by following those guidelines:

#### Search first in the existing database

Issues are often reported several times by various users. It's a good practice
to **search first** in the issues database before reporting your issue. If you
don't find a relevant match or if you are unsure, don't hesitate to **open a
new issue**. The bugsquad will handle it from there if it's a duplicate.

#### Specify steps to reproduce

Many bugs can't be reproduced unless specific steps are taken. Please **specify
the exact steps** that must be taken to reproduce the condition, and try to
keep them as minimal as possible.

## Contributing pull requests

If you want to add new functionalities, please make sure that:

* This functionality is desired.
* You talked to other developers on how to implement it best (on either
  communication channel, and maybe in a GitHub issue first before making your
  PR).
* Even if it does not get merged, your PR is useful for future work by another
  developer.

Similar rules can be applied when contributing bug fixes - it's always best to
discuss the implementation in the bug report first if you are not 100% about
what would be the best fix.

#### Be nice to the git history

Try to make simple PRs with that handle one specific topic. Just like for
reporting issues, it's better to open 3 different PRs that each address a
different issue than one big PR with three commits.

When updating your fork with upstream changes, please use ``git pull --rebase``
to avoid creating "merge commits". Those commits unnecessarily pollute the git
history when coming from PRs.

Also try to make commits that bring the engine from one stable state to another
stable state, i.e. if your first commit has a bug that you fixed in the second
commit, try to merge them together before making your pull request (see ``git
rebase -i`` and relevant help about rebasing or amending commits on the
Internet).

This git style guide has some good practices to have in mind:
[Git Style Guide](https://github.com/agis-/git-style-guide)

#### Format your commit logs with readability in mind

The way you format your commit logs is quite important to ensure that the
commit history and changelog will be easy to read and understand. A git commit
log is formatted as a short title (first line) and an extended description
(everything after the first line and an empty separation line).

The short title is the most important part, as it is what will appear in the
`shortlog` changelog (one line per commit, so no description shown) or in the
GitHub interface unless you click the "expand" button. As the name tells it,
try to keep that first line relatively short (ideally <= 50 chars, though it's
rare to be able to tell enough in so few characters, so you can go a bit
higher) - it should describe what the commit does globally, while details would
go in the description. Typically, if you can't keep the title short because you
have too much stuff to mention, it means that you should probably split your
changes in several commits :)

Here's an example of a well-formatted commit log (note how the extended
description is also manually wrapped at 80 chars for readability):

Thanks in advance!
