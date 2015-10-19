# SpriterDotNet
A simple, fast and efficient Spriter implementation in pure C#

## About
The goal of SpriterDotNet is to be as fast as possible while keeping the code clean and readable. 
Being a pure C# implementation, SpriterDotNet doesn't depend on any external libraries / frameworks. It focuses on simple and efficient calculations of all transforms at a given point in time. This allows using it with any framework just by mapping calculated transforms to concrete objects.

## Supported Features
* Basic animations
* Bone animations
* All curve types (Instant, Linear, Quadratic, Cubic, Quartic, Quintic, Bezier)
* Points
* Collision Rectangles
* SubEntities
* Events
* Sounds
* Variables
* Tags
* Character maps
* Animation Blending

## How to use it
1. Extend SpriterAnimator<TSprite, TSound> with generic parameters being the concrete types for the framework you're using and override ApplySpriteTransform and PlaySound methods
2. Obtain a string with the SCML data
3. Get a Spriter instance with SpriterParser.Parse
4. Instantiate your SpriterAnimator class with the desired Entity
5. Register concrete objects which correspond to FolderId/FileId
6. Call Step in every frame
7. Control the animation with properties

#### Points
* Override SpriterAnimator.ApplyPointTransform

#### Collision Rectangles
* Override SpriterAnimator.ApplyBoxTransform

#### Events
* Subscribe to the SpriterAnimator.EventTriggered event

#### Variables
* Query SpriterAnimator.Metadata

#### Tags
* Query SpriterAnimator.Metadata

#### Character Maps
* Set SpriterAnimator.CharacterMap to desired value or null

#### Animation Blending
* Call SpriterAnimator.Transition or SpriterAnimator.Blend

**Animation blending is possible only between animations with identical hierarchies. Blending incompatible animations will cause strange behaviour. SpriterDotNet only performs a simple check to determine compatibility in order to avoid crashing but that might not be enough in some cases.**

## Feedback
For questions, feedback, complaints, etc, use the related topic on [Spriter Forum](http://brashmonkey.com/forum/index.php?/topic/4166-spriterdotnet-an-implementation-for-all-c-frameworks/)

Also, feel free to drop a note if you use SpriterDotNet in your game/project and I'll be happy to add a showcase section with links to your game/project.
