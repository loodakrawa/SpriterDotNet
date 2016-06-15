# SpriterDotNet
A simple, fast and efficient Spriter implementation in pure C#

## About
The goal of SpriterDotNet is to be as fast as possible while keeping the code clean and readable. 
Being a pure C# implementation, SpriterDotNet doesn't depend on any external libraries / frameworks. It focuses on simple and efficient calculations of all transforms at a given point in time. This allows using it with any framework just by mapping calculated transforms to concrete objects.

## Completed Plugins
* [Unity](SpriterDotNet.Unity)
* [MonoGame](SpriterDotNet.MonoGame)

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

## Using SpriterDotNet
Refer to the specific documentation for each plugin.

## Using SpriterDotNet with any engine
There are a lot of different ways of using this plugin but this is probably the most efficient for the majority of scenarios:

1. Extend [Animator<TSprite, TSound>](SpriterDotNet/Animator.cs) with generic parameters being the concrete types for the framework you're using and override ApplyTransform and PlaySound methods
2. Obtain a string with the SCML data
3. Get a Spriter instance by calling [SpriterReader.Default.Read](SpriterDotNet/SpriterReader.cs) on the string from the previous step
4. Instantiate a [DefaultProviderFactory<Texture2D, SoundEffect>](SpriterDotNet/Providers/DefaultProviderFactory.cs)
5. Load the required TSprites and TSounds based on the FolderId/FileId from the Spriter instance and register them with the DefaultProviderFactory
6. Instantiate your Animator with the desired SpriterEntity and the DefaultProviderFactory instance
7. Call Animator.Step every frame
8. Control the animation with [Animator properties](#animator)

For already implemented plugins refer to their own documentation pages.

## Details and Customisation
SpriterDotNet's default configuration should be good enough for most users but it is designed in a way that allows customising almost everything.

### [FrameData](SpriterDotNet/FrameData.cs)
FrameData contains all the information about the state of the animation (or blend of multiple animations) at a certain point in time.

### [Config](SpriterDotNet/Config.cs)
The config is used to configure common properties of default implementations.

### [Providers](SpriterDotNet/Providers)
The Animator uses providers to get Sprites, Sounds and data for every frame.

###### [Provider Factory](SpriterDotNet/IProviderFactory.cs)
The ProviderFactory is responsible for constructing/pooling/reusing provider instances. An instance of IProviderFactory can be passed as an optional argument when constructing the Animator.
[The default implementation](SpriterDotNet/Providers/DefaultProviderFactory.cs) is designed to:
* Share asset providers between all animators operating on Entities from the same Spriter file
* Share the SnapshotFrameDataProvider between all animators operating on the same Entity (can be enabled via constructor flag)

###### [Asset Provider](SpriterDotNet/IAssetProvider.cs)
AssetProviders are responsible for providing Sprites and Sounds and for taking care of all the relevant manipulations (like applying character maps).
They are exposed as properties in the Animator and can be swapped with customised implementations.

###### [Frame Data Provider](SpriterDotNet/IFrameDataProvider.cs)
The Frame Data Provider is responsible for providing FrameData for the given point in time. SpriterDotNet comes with these implementations:
* [The default implementation] (SpriterDotNet/Providers/DefaultFrameDataProvider.cs) simply calculates everything for each time it gets called
* [The snapshot implementation] (SpriterDotNet/Providers/SnapshotFrameDataProvider.cs) takes snapshots at certain intervals and returns the closest snapshot when called. This means that this implementation requires more memory but there is virtually no processing required. Uses the default implementation under the hood. Also, caching results from blending animations would require too much memory so it just falls back to the default implementation for blends

### [Animator](SpriterDotNet/Animator.cs)
This class contains the majority of Properties and Methods necessary to control the animation.

###### Properties
* Speed - Playback speed. Negative speeds reverse the animation
* Time - The current time in animation (in milliseconds)
* Progress - The progress of animation ([0...1])
* FrameData - The latest FrameData
* SpriteProvider - IAssetProvider for sprites
* SoundProvider - IAssetProvider for sounds

######  Methods
* Play - Plays the given animation
* Transition - Transitions to given animation doing a progressive blend in the given time
* Blend - Blends two animations with the given weight factor

**Animation blending is possible only between animations with identical hierarchies. Blending incompatible animations will cause strange behaviour. SpriterDotNet only performs a simple check to determine compatibility in order to avoid crashing but that might not be enough in some cases.**

### Parsing and Initialisation
All the parsing and processing is done through a [SpriterReader](SpriterDotNet/SpriterReader.cs) instance. This class has a collection of [ISpriterParsers](SpriterDotNet/ISpriterParser.cs) and [ISpriterPreprocessors](SpriterDotNet/ISpriterPreprocessor.cs). The Read method calls all the registered parsers in sequence until the first parsing success. Then it iterates over all preprocessors invoking them on the spriter instance. SpriterDotNet comes with these default implementations:
* [XmlSpriterParser](SpriterDotNet/Parsers/XmlSpriterParser.cs) - the parser for the .scml file format
* [SpriterInitPreprocessor](SpriterDotNet/Preprocessors/SpriterInitPreprocessor.cs) - the preprocessor for initialising the default values for the Spriter data hierarchy

### Other Features
* Points - Controlled in Animator.ApplyPointTransform
* Collision Rectangles - Controlled in Animator.ApplyBoxTransform
* Events - Exposed as a C# event - Animator.EventTriggered
* Variables - Exposed in Animator.FrameData.AnimationVars and Animator.FrameData.ObjectVars
* Tags - Exposed in Animator.FrameData.AnimationTags and Animator.FrameData.ObjectTags
* Character Maps - Manipulated through Animator.SpriteProvider and Animator.SoundProvider
* Animation Blending - Animator.Transition or Animator.Blend

## Feedback
For questions, feedback, complaints, etc, use the related topic on [Spriter Forum](http://brashmonkey.com/forum/index.php?/topic/4166-spriterdotnet-an-implementation-for-all-c-frameworks/)

Also, feel free to drop a note if you use SpriterDotNet in your game/project and I'll be happy to add a showcase section with links to your game/project.
