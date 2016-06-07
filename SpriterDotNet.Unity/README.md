# SpriterDotNet.Unity

## Installation
The easiest way to install SpriterDotNet.Unity is by importing the appropriate .unitypackage.
This distribution comes with 2 different packages:
* SpriterDotNet.Unity.unitypackage (contains just the library)
* SpriterDotNet.Unity.Examples.unitypackage (contains the library and a couple of examples)

## Basic Usage
The most easy way of using SpriterDotNet.Unity with your own animations is the following:
* Install SpriterDotNet.Unity.Examples.unitypackage
* Create a new scene
* Copy your scml and all required files into the Assets folder in your project
* Drag and drop the generated .prefab to the scene (SpriterDotNet.Unity generates a prefab for each entity in the scml)
* Drag and drop Controller.prefab to the scene
* (Optional) Drag and drop Canvas.prefab to the scene to have on-screen instructions about key mappings

**Note that the example Controller supports ony one prefab per scene. If you add multiples, it's only going to affect one of them.**

## Usage
The usual way of using SpriterDotNet.Unity is to get a reference to a [UnityAnimator](Assets/SpriterDotNet/UnityAnimator.cs) from a SpriterDotNetBehaviour and controlling the animation as described in the [Animator documentation](../README.md#animator).

###### Getting a reference tp UnityAnimator
In a script attached to the same GameObject which also contains SpriterDotNetBehaviour:
```csharp
UnityAnimator animator = gameObject.GetComponent<SpriterDotNetBehaviour>().Animator;
```

In a script attached to other GameObjects:
```csharp
UnityAnimator animator = FindObjectOfType<SpriterDotNetBehaviour>().Animator;
```

## Tags
SpriterDotNet creates Unity tags while importing Spriter assets. This behaviour can be controlled with the UseNativeTags 
flag in SpriterImporter. Unfortunately Unity doesn't support multiple tags so SpriterDotNet just sets the first tag as the GameObject tag. Other tags can be accessed via the FrameData property of UnityAnimator.

## Customising importing
The SpriterDotNet.Unity plugin exposes the following points for customising importing:
* SpriterImporter.EntityImported event - gets called when an SpriterEntity gets created
* SpriterImporter.ContentLoader - the loader that loads Sprites and Sounds - can be substituted with custom implementation

For examples, see TestSpriterImportHook in SpriterDotNEtExamples. 

## Generated ScriptableObjects
SpriterDotNet.Unity generates one ScriptableObject per .scml file to hold all parsed scml data which is shared across all entities defined in the same scml. (For example, for squares.scml a squares.asset object gets generated). This means that for most changes to the .scml get reflected in Unity automatically.

## Features
This plugin has been designed with most common scenarios in mind. However, it's hard to cover all possible use-cases. 
If you have good ideas about improving functionality and/or ease of use feel free to suggest it either on [Spriter Forum](http://brashmonkey.com/forum/index.php?/topic/4166-spriterdotnet-an-implementation-for-all-c-frameworks/)
or here on GitHub (as an Enhancement Issue).
