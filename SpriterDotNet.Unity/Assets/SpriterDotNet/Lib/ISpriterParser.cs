// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    public interface ISpriterParser
    {
        /// <summary>
        /// Indicates whether the parser knows how to parse the input string.
        /// </summary>
        bool CanParse(string data);

        /// <summary>
        /// Parses the input string and returns a Spriter instance.
        /// </summary>
        Spriter Parse(string data);
    }
}
