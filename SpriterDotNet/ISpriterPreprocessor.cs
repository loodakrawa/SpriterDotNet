// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    public interface ISpriterPreprocessor
    {
        /// <summary>
        /// Does some kind of preprocessing on the Spriter instance after parsing and before returning it to the user - e.g. initialisation logic.
        /// </summary>
        void Preprocess(Spriter spriter);
    }
}
