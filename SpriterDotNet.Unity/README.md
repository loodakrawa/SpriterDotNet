# SpriterDotNet.Unity

## Installation
The easiest way to install SpriterDotNet.Unity is by importing the appropriate .unitypackage.
This distribution comes with 2 different packages:
* SpriterDotNet.Unity.unitypackage (contains just the library)
* SpriterDotNet.Unity.Examples.unitypackage (contains the library and a couple of examples)

## Tags
SpriterDotNet creates Unity tags while importing Spriter assets. This behaviour can be controlled with the UseNativeTags 
flag in SpriterImporter. Unfortunately Unity doesn't support multiple tags so SpriterDotNet just sets the first tag as the
GameObject tag. Other tags can be accessed via the Metadata property of UnitySpriterAnimator.

## Importer Hooks
If you want to do post processing on imported assets, SpriterImporter exposes the EntityImported event. See
TestSpriterImportHook in the examples project.

## Usability
This plugin has been designed with most common scenarios in mind. However, it's hard to cover all possible use-cases. 
If you have good ideas about improving functionality and/or ease of use feel free to suggest it either on [Spriter Forum](http://brashmonkey.com/forum/index.php?/topic/4166-spriterdotnet-an-implementation-for-all-c-frameworks/)
or here on GitHub (as an Enhancement Issue).
