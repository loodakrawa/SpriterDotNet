using SpriterDotNet;
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
    public class SpriterDotNetBehaviour : MonoBehaviour
    {
        public float MaxSpeed = 5.0f;
        public float DeltaSpeed = 0.2f;

        public SdnFolder[] Folders;
        public GameObject[] Pivots;
        public GameObject[] Children;

        private UnitySpriterAnimator animator;
        private SpriterEntity entity;

        [HideInInspector]
        public string SpriterData;

        public void Awake()
        {
            Spriter spriter = Spriter.Parse(SpriterData);
            entity = spriter.Entities[0];
            animator = new UnitySpriterAnimator(entity, Pivots, Children);
            RegisterSprites();

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) animator.Step(Time.deltaTime);
#endif
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif

            animator.Step(Time.deltaTime * 1000.0f);

            if (Input.GetButtonDown("space")) NextAnimation();
            if (Input.GetButtonDown("r")) animator.Speed = -animator.Speed;
            if (Input.GetButtonDown("p")) ChangeAnimationSpeed(DeltaSpeed);
            if (Input.GetButtonDown("o")) ChangeAnimationSpeed(-DeltaSpeed);
        }

        private void NextAnimation()
        {
            List<string> animations = animator.GetAnimations().ToList();
            int index = animations.IndexOf(animator.CurrentAnimation.Name);
            ++index;
            if (index >= animations.Count) index = 0;
            animator.Play(animations[index]);
        }

        private void ChangeAnimationSpeed(float delta)
        {
            var speed = animator.Speed + delta;
            speed = Math.Abs(speed) < MaxSpeed ? speed : MaxSpeed * Math.Sign(speed);
            animator.Speed = speed;
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
    }

}
