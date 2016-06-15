# SpriterDotNet.MonoGame
This contains only MonoGame specific things. Refer to the [main doc](../README.md) for everything else.

## Installation
To install SpriterDotNet.MonoGame either install the [NuGet Package](https://www.nuget.org/packages/SpriterDotNet.MonoGame/) or clone the repo and add SpriterDotNet.MonoGame as a project reference.

## Usage
* Follow the steps 2 to 8 from [generic usage](../README.md#using-spriterdotnet-with-any-engine)
* For step 6. instantiate [MonoGameAnimator](MonoGameAnimator.cs)
* As an additional step, call MonoGameAnimator.Draw every frame


## [MonoGameAnimator](MonoGameAnimator.cs)
The MonoGame specific implementation of [Animator](../SpriterDotNet/Animator.cs) with Texture2D and SoundEffect as the generic types. It operates in two phases. During Update, it calculates the transforms for every Texture2D and stores them into a buffer  which gets rendered during Draw.

###### MonoGame specific properties
* Scale - the scale of the whole animator
* Rotation - rotation (in radians)
* Position - position on the screen (in pixels)
* Depth - the rendering depth ([0...1])
* DeltaDepth - the depth difference between individual sprites of the animation