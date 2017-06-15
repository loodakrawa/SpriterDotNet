// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriterDotNet.Example
{
    public static class AnimatorExtensions
    {
        public static void PushNextCharacterMap<TSprite, TSound>(this Animator<TSprite, TSound> animator)
        {
            SpriterCharacterMap[] maps = animator.Entity.CharacterMaps;
            if (maps == null || maps.Length == 0) return;
            SpriterCharacterMap charMap = animator.SpriteProvider.CharacterMap;
            if (charMap == null) charMap = maps[0];
            else
            {
                int index = charMap.Id + 1;
                if (index >= maps.Length) charMap = null;
                else charMap = maps[index];
            }

            if (charMap != null) animator.SpriteProvider.PushCharMap(charMap);
        }

        public static void PopCharacterMap<TSprite, TSound>(this Animator<TSprite, TSound> animator)
        {
            animator.SpriteProvider.PopCharMap();
        }

        public static string GetVarValues<TSprite, TSound>(this Animator<TSprite, TSound> animator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in animator.FrameData.AnimationVars)
            {
                object value = GetValue(entry.Value);
                sb.Append(entry.Key).Append(" = ").AppendLine(value.ToString());
            }
            foreach (var objectEntry in animator.FrameData.ObjectVars)
            {
                foreach (var varEntry in objectEntry.Value)
                {
                    object value = GetValue(varEntry.Value);
                    sb.Append(objectEntry.Key).Append(".").Append(varEntry.Key).Append(" = ").AppendLine((value ?? string.Empty).ToString());
                }
            }

            return sb.ToString();
        }

        public static string GetTagValues<TSprite, TSound>(this Animator<TSprite, TSound> animator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string tag in animator.FrameData.AnimationTags) sb.AppendLine(tag);
            foreach (var objectEntry in animator.FrameData.ObjectTags)
            {
                foreach (string tag in objectEntry.Value) sb.Append(objectEntry.Key).Append(".").AppendLine(tag);
            }

            return sb.ToString();
        }

        public static string GetNextAnimation<TSprite, TSound>(this Animator<TSprite, TSound> animator)
        {
            List<string> animations = animator.GetAnimations().ToList();
            int index = animations.IndexOf(animator.CurrentAnimation.Name);
            ++index;
            if (index >= animations.Count) index = 0;
            return animations[index];
        }

        public static void ChangeAnimationSpeed<TSprite, TSound>(this Animator<TSprite, TSound> animator, float delta, float max)
        {
            var speed = animator.Speed + delta;
            speed = Math.Abs(speed) < 5.0f ? speed : 5.0f * Math.Sign(speed);
            animator.Speed = speed;
        }

        private static object GetValue(SpriterVarValue varValue)
        {
            object value;
            switch (varValue.Type)
            {
                case SpriterVarType.Float:
                    value = varValue.FloatValue;
                    break;
                case SpriterVarType.Int:
                    value = varValue.IntValue;
                    break;
                default:
                    value = varValue.StringValue;
                    break;
            }
            return value;
        }

    }
}
