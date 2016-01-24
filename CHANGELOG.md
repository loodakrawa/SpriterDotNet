##### 1.3
* Convert SpriterDotNet to a PCL

##### 1.2.2
* Cache Unity transforms
* Add Metadata to SnapshotAnimationDataProvider

##### 1.2.1
* Fix bug where the subentity doesn't have any bones
* Fix bug with object pool for arrays of size 0
* Add Scale to MonogameSpriterAnimator

##### 1.2.0
* Add support for animation data providers
* Fix bug that caused sprite color to reset on every frame
* Use sortOrder instead of z coordinate for z-index
* Add ability to choose sorting layer

##### 1.1.2
* Reduce garbage generation to 0

##### 1.1.1
* Unity importer retains components during prefab replace
* Add sprite swapping capabilities

##### 1.1.0
* Add flags to control Metadata calculations
* Unity importer retains transform values during prefab replace
* Fix bug when Animation has no Timelines
* Fix bug when Entity has no ObjectInfos

##### 1.0.4
* Fix SpriterImporter to pass prefab instead of destroyed tmp object

##### 1.0.3
* Fix bug when no keyframe in t = 0
* Fix bug when keyframe has no object_refs

##### 1.0.2
* Fix changing pivots when blending animations

##### 1.0.1
* Add XML Documentation to SpriterAnimator
* Fix blend compatibility test
* Fix read only properties in SpriterAnimator
* Fix bug with weird behaviour on extremely short transition time
* Move first animation init from SpriterAnimator cosntructor to Step method