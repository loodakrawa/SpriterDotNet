﻿using SpriterDotNet;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SpriterDotNetUnity
{
    [Serializable]
    public class SdnFolder
    {
        public Sprite[] Files;
    }

    [ExecuteInEditMode]
    public class SpriterDotNetBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {
        public float AnimatorSpeed = 1.0f;
        public float MaxSpeed = 5.0f;
        public float DeltaSpeed = 0.2f;

        public SdnFolder[] Folders;
        public GameObject[] Pivots;
        public GameObject[] Children;

        [HideInInspector]
        public string SpriterData;

        private UnitySpriterAnimator animator;
        private SpriterEntity entity;

        private bool deserializationComplete;

        public void Awake()
        {
			#if UNITY_EDITOR
			if(deserializationComplete){
				Spriter spriter = Spriter.Parse(SpriterData);
				entity = spriter.Entities[0];
				animator = new UnitySpriterAnimator(entity, Pivots, Children);
				RegisterSprites();
				if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) animator.Step(Time.deltaTime);
			}
			#endif
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
            animator.Speed = AnimatorSpeed;
            animator.Step(Time.deltaTime * 1000.0f);
            if (GetAxisDownPositive("Horizontal")) ChangeAnimation(1);
            if (GetAxisDownNegative("Horizontal")) ChangeAnimation(-1);
            if (GetAxisDownPositive("Vertical")) ChangeAnimationSpeed(DeltaSpeed);
            if (GetAxisDownNegative("Vertical")) ChangeAnimationSpeed(-DeltaSpeed);
            if (Input.GetButtonDown("Jump")) ReverseAnimation();
        }

        private static bool GetAxisDownPositive(string axisName)
        {
            return Input.GetButtonDown(axisName) && Input.GetAxis(axisName) > 0;
        }

        private static bool GetAxisDownNegative(string axisName)
        {
            return Input.GetButtonDown(axisName) && Input.GetAxis(axisName) < 0;
        }

        private void ChangeAnimation(int delta)
        {
            List<string> animations = animator.GetAnimations().ToList();
            int index = animations.IndexOf(animator.CurrentAnimation.Name);
            index += delta;
            if (index >= animations.Count) index = 0;
            if (index < 0) index = animations.Count - 1;
            animator.Play(animations[index]);
        }

        private void ChangeAnimationSpeed(float delta)
        {
            var speed = animator.Speed + delta;
            speed = Math.Abs(speed) < MaxSpeed ? speed : MaxSpeed * Math.Sign(speed);
            AnimatorSpeed = (float)Math.Round(speed, 1, MidpointRounding.AwayFromZero);
        }

        private void ReverseAnimation()
        {
            AnimatorSpeed *= -1;
        }

        private void RegisterSprites()
        {
            for (int i = 0; i < Folders.Length; ++i)
            {
                Sprite[] files = Folders[i].Files;
                for (int j = 0; j < files.Length; ++j)
                {
                    animator.Register(i, j, files[j]);
                }
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            deserializationComplete = true;
        }
    }
}
