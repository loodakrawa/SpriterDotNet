using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SpriterDotNet.Parser;

namespace SpriterDotNet.UnitTests.ForParsing
{
    [TestFixture]
    public class ForSpriterParser
    {
        [Test]
        public void WhenInstantiated_HasDefaultParsers()
        {
            var parsers = SpriterParser.Parsers;
            parsers.Any(p => p.GetType() == typeof(XmlSpriterParser)).Should().BeTrue("the parsers should contain an XmlSpriterParser");
        }

        [TestCase(null, "the input data was null")]
        [TestCase("", "the input data was empty")]
        [TestCase("   ", "the input data was whitespace")]
        public void Parse_WithNullOrEmptyData_ReturnsNull(string data, string because)
        {
            SpriterParser.Parse(data).Should().BeNull(because);
        }

        [Test]
        public void Parse_IfAMatchIsFoundInParsers_AnAttemptWillBeMadeToParseTheDataWithTheMatchedParser()
        {
            using (new SpriterParserTestScope())
            {
                var parser = new Mock<ISpriterParser>();
                parser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);
                parser.Setup(p => p.Parse(It.IsAny<string>())).Returns((Spriter)null);

                SpriterParser.Parsers.Clear();
                SpriterParser.Parsers.Add(parser.Object);
                SpriterParser.Parse("DATA");

                parser.Verify(p => p.CanParse(It.IsAny<string>()), Times.Once);
                parser.Verify(p => p.Parse(It.IsAny<string>()), Times.Once);
            }
        }

        [Test]
        public void Parse_IfAMatchIsNotFoundInParsers_NoAttemptWillBeMadeToParseTheData()
        {
            using (new SpriterParserTestScope())
            {
                var parser = new Mock<ISpriterParser>();
                parser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(false);
                parser.Setup(p => p.Parse(It.IsAny<string>())).Returns((Spriter)null);

                SpriterParser.Parsers.Clear();
                SpriterParser.Parsers.Add(parser.Object);
                SpriterParser.Parse("DATA");

                parser.Verify(p => p.CanParse(It.IsAny<string>()), Times.Once);
                parser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
            }
        }

        [Test]
        public void Init_IfCalledOnASpriterWithAFullHierarchy_SetsTimelineKeyObjectInfosToThePivotsFromTheFiles()
        {
            var spriter = new Spriter()
            {
                Folders = new[]
                {
                        new SpriterFolder()
                        {
                            Id = 0,
                            Files = new[]
                            {
                                new SpriterFile()
                                {
                                    Id = 0,
                                    PivotX = 0.25f,
                                    PivotY = 0.75f
                                }
                            }
                        }
                    },
                Entities = new[]
                {
                    new SpriterEntity()
                    {
                        Animations = new []
                        {
                            new SpriterAnimation()
                            {
                                Timelines = new []
                                {
                                    new SpriterTimeLine()
                                    {
                                        Keys = new[]
                                        {
                                            new SpriterTimeLineKey()
                                            {
                                                ObjectInfo = new SpriterObjectInfo ()
                                                {
                                                    FolderId = 0,
                                                    FileId = 0,
                                                    PivotX = float.NaN,
                                                    PivotY = float.NaN
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            SpriterParser.Init(spriter);

            var objectInfo = spriter.Entities.First().Animations.First().Timelines.First().Keys.First().ObjectInfo;
            objectInfo.PivotX.Should().Be(0.25f);
            objectInfo.PivotY.Should().Be(0.75f);
        }

        private class SpriterParserTestScope : IDisposable
        {
            private bool disposedValue = false;
            private readonly List<ISpriterParser> originalParsers;
            private readonly Action<Spriter> originalInit;

            public SpriterParserTestScope()
            {
                originalParsers = SpriterParser.Parsers.ToList();
            }

            private void Dispose(bool disposing)
            {
                if (disposedValue) return;

                if (disposing)
                {
                    SpriterParser.Parsers.Clear();
                    originalParsers.ForEach(p => SpriterParser.Parsers.Add(p));
                }
                disposedValue = true;
            }

            public void Dispose()
            {
                Dispose(true);
            }
        }
    }
}
