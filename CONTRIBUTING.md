# How to contribute
All contributors welcome - the more the merrier. Just try following these simple guidelines.

## Changes
Feel free to implement any bug fixes and small changes. However, if you want to do massive and/or breaking changes, please discuss it first. It would be a shame to have lots of work going to waste.

## Code style
Try following the DRY, KISS and YAGNI principles, and most important - common sense.

##### Naming
Try following the [Microsoft Naming Guidelines](https://msdn.microsoft.com/en-us/library/ms229002%28v=vs.110%29.aspx)

##### Formatting
Try to use something similar to the one used throughout the existing code. Visual Studio's Ctrl+K+D does a really good job in this regard.

## File headers
If you add new files, please add the following header in the beginning of the file, replacing the {YEAR} placeholder with the current year:
```
// Copyright (c) {YEAR} The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.
```

## Git flow

In general, this is the recommended way of doing things:

1. Develop new stuff in a feature branch branched from develop (e.g. feature/name-of-feature)
2. After dev is done, squash commits into one (or a few meaningful ones)
3. Sync with the upstream repo
4. Merge develop into feature branch and fix all conflicts
5. Create pull request from your feature branch to upstream develop
6. Sync your repo with the upstream one after your pull request gets merged

##### Pull requests
Describe what the pull request does as best as you can.

##### Additional resources:

- [Tutorial about forking workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/forking-workflow/)
- [Squashing commits](http://stackoverflow.com/a/5201642)
