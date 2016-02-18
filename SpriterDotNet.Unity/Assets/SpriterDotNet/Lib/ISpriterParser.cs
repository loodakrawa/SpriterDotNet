// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    public interface ISpriterParser
    {
        Spriter Parse(string data);
        bool CanParse(string data);
    }
}
