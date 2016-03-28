// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SpriterDotNet.MonoGame.Example
{
    public class GameStateManager
    {
        private readonly Stack<GameState> gameStates = new Stack<GameState>();
        private GameState currentState;

        public void Push(GameState state)
        {
            state.GameStateManager = this;
            if (!state.Loaded) state.BaseLoad();
            gameStates.Push(state);
            currentState = state;
        }

        public GameState Pop()
        {
            if (gameStates.Count == 0) return null;
            var removed = gameStates.Pop();
            currentState = gameStates.Count > 0 ? gameStates.Peek() : null;
            if (removed.Loaded) removed.BaseUnload();
            return removed;
        }

        public void Update(GameTime gameTime)
        {
            if (currentState == null) return;
            currentState.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            if (currentState == null) return;
            currentState.Draw(gameTime);
        }
    }
}