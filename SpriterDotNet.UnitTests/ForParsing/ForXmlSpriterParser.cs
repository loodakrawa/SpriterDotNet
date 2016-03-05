// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SpriterDotNet.Parser;

namespace SpriterDotNet.UnitTests.ForParsing
{
    [TestFixture]
    public class ForXmlSpriterParser
    {
        [Test]
        public void CanParse_IfStringStartsWithAngleBracket_ReturnsTrue()
        {
            new XmlSpriterParser().CanParse("<").Should().BeTrue();
        }

        [Test]
        public void CanParse_IfStringIsNotXml_ReturnsFalse()
        {
            new XmlSpriterParser().CanParse("Some String").Should().BeFalse();
        }

        [Test]
        public void CanParse_IfStringIsNull_ThrowsANullReferenceException()
        {
            Assert.Throws<NullReferenceException>(() => { new XmlSpriterParser().CanParse(null); });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseSpriterWithFoldersAndEntities()
        {
            var spriterScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <folder></folder>
    <folder></folder>
    <folder></folder>
    <entity></entity>
    <entity></entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(spriterScml);
            spriter.ShouldBeEquivalentTo(new Spriter()
            {
                Folders = new[]
                {
                    new SpriterFolder(),
                    new SpriterFolder(),
                    new SpriterFolder()
                },
                Entities = new[]{
                    new SpriterEntity(),
                    new SpriterEntity()
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseFoldersAndFiles()
        {
            var folderScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <folder id=""3"" name=""torso"">
        <file/>
        <file/>
        <file/>
    </folder>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(folderScml);
            var folder = spriter.Folders.First();
            folder.ShouldBeEquivalentTo(new SpriterFolder()
            {
                Id = 3,
                Name = "torso",
                Files = new[]
                {
                    new SpriterFile(),
                    new SpriterFile(),
                    new SpriterFile()
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseFiles()
        {
            var fileScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <folder>
        <file id=""3"" name=""torso/p_torso_front.png"" width=""88"" height=""89"" pivot_x=""0.877778"" pivot_y=""0.511111""/>
    </folder>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(fileScml);
            var file = spriter.Folders.First().Files.First();
            file.ShouldBeEquivalentTo(new SpriterFile()
            {
                Id=3,
                Name="torso/p_torso_front.png",
                Width = 88,
                Height = 89,
                PivotX = 0.877778f,
                PivotY = 0.511111f
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseEntityAndAnimations()
        {
            var entityScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity id=""4"" name=""Player"">
        <animation></animation>
        <animation></animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(entityScml);
            var entity = spriter.Entities.First();
            entity.ShouldBeEquivalentTo(new SpriterEntity ()
            {
                Id = 4,
                Name = "Player",
                Animations = new []
                {
                    new SpriterAnimation(),
                    new SpriterAnimation()
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseAnimationMainlineKeysAndTimelines()
        {
            var animationScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation id=""1"" name=""walk"" length=""1000"">
	        <mainline>
		        <key></key>
		        <key></key>
	        </mainline>
	        <timeline></timeline>
	        <timeline></timeline>
	        <timeline></timeline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(animationScml);
            var animation = spriter.Entities.First().Animations.First();
            animation.ShouldBeEquivalentTo(new SpriterAnimation()
            {
                Id=1,
                Name="walk",
                Length = 1000,
                MainlineKeys = new []
                {
                    new SpriterMainlineKey(),
                    new SpriterMainlineKey()
                },
                Timelines = new []
                {
                    new SpriterTimeline(),
                    new SpriterTimeline(),
                    new SpriterTimeline()
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseMainlineKeyBoneRefsAndObjectRefs()
        {
            var mainlineKeyScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <mainline>
		        <key id=""3"">
                    <bone_ref></bone_ref>
                    <bone_ref></bone_ref>
                    <bone_ref></bone_ref>
                    <object_ref></object_ref>
                    <object_ref></object_ref>
                </key>
                <key id=""4"" name=""some_key"" time=""2000"" curve_type=""cubic"" c1=""1.0"" c2=""2.0"" c3=""3.0"" c4=""4.0""></key>
            </mainline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(mainlineKeyScml);
            var mainlineKeys = spriter.Entities.First().Animations.First().MainlineKeys;
            mainlineKeys.ShouldBeEquivalentTo(new[]
            {
                new SpriterMainlineKey()
                {
                    Id=3,
                    Name = null,
                    Time = 0,
                    CurveType = SpriterCurveType.Linear,
                    C1 = 0.0f,
                    C2 = 0.0f,
                    C3 = 0.0f,
                    C4 = 0.0f,
                    BoneRefs = new []
                    {
                        new SpriterRef(),
                        new SpriterRef(),
                        new SpriterRef()
                    },
                    ObjectRefs = new []
                    {
                        new SpriterObjectRef(),
                        new SpriterObjectRef()
                    }
                },
                new SpriterMainlineKey()
                {
                    Id=4,
                    Name = "some_key",
                    Time = 2000f,
                    CurveType = SpriterCurveType.Cubic,
                    C1 = 1.0f,
                    C2 = 2.0f,
                    C3 = 3.0f,
                    C4 = 4.0f,
                    BoneRefs = null,
                    ObjectRefs = null
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseBoneRefs()
        {
            var boneRefScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <mainline>
		        <key>
                    <bone_ref id=""0"" timeline=""15"" key=""0""/>
                    <bone_ref id=""1"" parent=""0"" timeline=""16"" key=""0""/>
                </key>
            </mainline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(boneRefScml);
            var boneRefs = spriter.Entities.First().Animations.First().MainlineKeys.First().BoneRefs;
            boneRefs.ShouldBeEquivalentTo(new []
            {
                new SpriterRef()
                {
                    Id=0,
                    ParentId = -1,
                    TimelineId = 15,
                    KeyId = 0
                },
                new SpriterRef()
                {
                    Id = 1,
                    ParentId = 0,
                    TimelineId = 16,
                    KeyId = 0
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseObjectRefs()
        {
            var objectRefScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <mainline>
		        <key>
                    <object_ref id=""0"" parent=""6"" name=""p_arm_idle_a"" folder=""2"" file=""0"" abs_x=""3.935046"" abs_y=""132.047933"" abs_pivot_x=""0.388889"" abs_pivot_y=""0.487179"" abs_angle=""239.547944"" abs_scale_x=""0.999999"" abs_scale_y=""1"" abs_a=""1"" timeline=""2"" key=""0"" z_index=""0""/>
                    <object_ref id=""1"" parent=""7"" name=""p_forearm_walk_a"" folder=""2"" file=""1"" abs_x=""-10.054159"" abs_y=""108.925523"" abs_pivot_x=""0.403846"" abs_pivot_y=""0.526316"" abs_angle=""250.330674"" abs_scale_x=""0.999999"" abs_scale_y=""1"" abs_a=""1"" timeline=""3"" key=""0"" z_index=""1""/>
                </key>
            </mainline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(objectRefScml);
            var objectRefs = spriter.Entities.First().Animations.First().MainlineKeys.First().ObjectRefs;
            objectRefs.ShouldBeEquivalentTo(new[]
            {
                new SpriterObjectRef()
                {
                    Id=0,
                    Name = "p_arm_idle_a",
                    ParentId = 6,
                    TimelineId = 2,
                    KeyId = 0,
                    ZIndex = 0
                },
                new SpriterObjectRef()
                {
                    Id=1,
                    Name = "p_forearm_walk_a",
                    ParentId = 7,
                    TimelineId = 3,
                    KeyId = 0,
                    ZIndex = 1
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseTimelinesAndTimelineKeys()
        {
            var mainlineKeyScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <timeline id=""8"" obj=""8"" name=""p_leg_a"">
                <key></key>
                <key></key>
            </timeline>
            <timeline id=""15"" obj=""15"" name=""pelvis"" object_type=""bone"">
                <key></key>
            </timeline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(mainlineKeyScml);
            var timelines = spriter.Entities.First().Animations.First().Timelines;
            timelines.ShouldBeEquivalentTo(new[]
            {
                new SpriterTimeline()
                {
                    Id=8,
                    ObjectId=8,
                    Name = "p_leg_a",
                    Keys = new []{
                        new SpriterTimelineKey(),
                        new SpriterTimelineKey()
                    },
                    ObjectType = SpriterObjectType.Sprite
                },
                new SpriterTimeline()
                {
                    Id = 15,
                    ObjectId=15,
                    Name = "pelvis",
                    Keys = new[]
                    {
                        new SpriterTimelineKey()
                    },
                    ObjectType = SpriterObjectType.Bone
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseValidTimelineKeysBoneInfoAndObjectInfo()
        {
            var mainlineKeyScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <timeline>
                <key id=""0"" spin=""0"">
                    <object/>
                </key>
                <key id=""1"">
                    <bone/>
                </key>
            </timeline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(mainlineKeyScml);
            var timelineKeys = spriter.Entities.First().Animations.First().Timelines.First().Keys;
            timelineKeys.ShouldBeEquivalentTo(new[]
            {
                new SpriterTimelineKey()
                {
                    Id=0,
                    Name = null,
                    Spin = 0,
                    ObjectInfo = new SpriterObject(),
                    BoneInfo = null
                },
                new SpriterTimelineKey()
                {
                    Id = 1,
                    Name = null,
                    Spin = 1,
                    ObjectInfo = null,
                    BoneInfo = new SpriterSpatial()
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseObjectInfo()
        {
            var objectInfoScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <timeline>
                <key>
                    <object folder=""3"" file=""0"" x=""-7.325194"" y=""-2.96502"" angle=""356.430565"" scale_x=""6.662966""/>
                </key>
                <key>
                    <object folder=""2"" file=""4"" x=""1.0"" y=""2.0"" angle=""90.0"" scale_y=""3.0""/>
                </key>
                <key>
                    <object folder=""7"" file=""6"" x=""2.0"" y=""1.0"" angle=""180.0"" a=""0.5""/>
                </key>
            </timeline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(objectInfoScml);
            var timelineKeyObjectInfos = spriter.Entities.First().Animations.First().Timelines.First().Keys.Select(k => k.ObjectInfo).ToArray();
            timelineKeyObjectInfos.ShouldBeEquivalentTo(new[] {
                new SpriterObject()
                {
                    FileId = 0,
                    FolderId = 3,
                    X = -7.325194f,
                    Y = -2.96502f,
                    Angle = 356.430565f,
                    ScaleX = 6.662966f,
                    ScaleY = 1.0f,
                    Alpha = 1.0f,
                    PivotX = float.NaN,
                    PivotY = float.NaN
                },
                new SpriterObject()
                {
                    FileId = 4,
                    FolderId = 2,
                    X = 1.0f,
                    Y = 2.0f,
                    Angle = 90.0f,
                    ScaleX = 1.0f,
                    ScaleY = 3.0f,
                    Alpha = 1.0f,
                    PivotX = float.NaN,
                    PivotY = float.NaN
                },
                new SpriterObject()
                {
                    FileId = 6,
                    FolderId = 7,
                    X = 2.0f,
                    Y = 1.0f,
                    Angle = 180.0f,
                    ScaleX = 1.0f,
                    ScaleY = 1.0f,
                    Alpha = 0.5f,
                    PivotX = float.NaN,
                    PivotY = float.NaN
                }
            });
        }

        [Test]
        public void Parse_IfXmlValid_CanParseBoneInfo()
        {
            var boneInfoScml = @"
<?xml version=""1.0"" encoding=""UTF-8""?>
<spriter_data>
    <entity>
        <animation>
	        <timeline>
                <key>
                    <bone x=""5"" y=""39.32879"" angle=""91.59114"" scale_x=""0.18527""/>
                </key>
                <key>
                    <bone x=""6"" y=""7"" angle=""0.0"" scale_y=""6.0""/>
                </key>
                <key>
                    <bone x=""9"" y=""8"" angle=""1.0"" a=""0.6""/>
                </key>
                <key>
                    <bone a=""NaN""/>
                </key>
                <key>
                    <bone a=""nan""/>
                </key>
            </timeline>
        </animation>
    </entity>
</spriter_data>
".Trim();

            var spriter = new XmlSpriterParser().Parse(boneInfoScml);
            var boneInfos = spriter.Entities.First().Animations.First().Timelines.First().Keys.Select(k => k.BoneInfo).ToArray();

            boneInfos.ShouldBeEquivalentTo(new[]
            {
                new SpriterSpatial()
                {
                    X=5,
                    Y = 39.32879f,
                    Angle = 91.59114f,
                    ScaleX = 0.18527f,
                    ScaleY = 1.0f,
                    Alpha = 1.0f
                },
                new SpriterSpatial()
                {
                    X=6.0f,
                    Y = 7.0f,
                    Angle = 0.0f,
                    ScaleX = 1.0f,
                    ScaleY = 6.0f,
                    Alpha = 1.0f
                },
                new SpriterSpatial()
                {
                    X=9.0f,
                    Y = 8.0f,
                    Angle = 1.0f,
                    ScaleX = 1.0f,
                    ScaleY = 1.0f,
                    Alpha = 0.6f
                },
                new SpriterSpatial()
                {
                    Alpha = float.NaN
                },
                new SpriterSpatial()
                {
                    Alpha = 0.0f
                }
            });
        }
    }
}
