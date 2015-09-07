# SpriterDotNet
A simple, fast and efficient Spriter implementation in pure C#

## About
The goal of SpriterDotNet is to be as fast as possible while keeping the code clean and readable. 
Being a pure C# implementation, SpriterDotNet doesn't depend on any external libraries / frameworks. It focuses on simple and efficient calculations of all transforms at a given point in time. This allows using it with any framework just by mapping calculated transforms to concrete objects.

## Supported Features
* Basic Animation
* Bone Animation
* Instant, Linear, Quadratic, Cubic, Quartic and Quintic curves

## How to use it
1. Obtain a string with the SCML data
2. Get a Spriter instance with Spriter.Parse
3. Invoke SpriterProcessor.GetDrawData with the desired animation and target time
4. Apply the calculated transforms to concrete objects in the framework you're using
5. goto 3.

```
Optionally, you can use SpriterAnimator<T> which is a basic player implementation.
To use it:

1. Extend it (with T being the concrete object type that gets transformed, e.g. Sprite)
2. Override the ApplyTransform method
3. Instantiate it with the desired SpriterEntity as argument
4. Register concrete objects which correspond to FolderId/FileId
5. Call Step in every frame
6. Control the animation with properties

* Take SpriterDotNet.Monogame as an example
```

## Feedback
For questions, feedback, complaints, etc, use the related topic on [Spriter Forum](http://brashmonkey.com/forum/index.php?/topic/4166-spriterdotnet-an-implementation-for-all-c-frameworks/)

Also, feel free to drop a note if you use SpriterDotNet in your game/project and I'll be happy to add a showcase section with links to your game/project.
