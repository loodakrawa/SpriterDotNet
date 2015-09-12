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
        public float animatorSpeed = 1.0f;
        public float maxSpeed = 5.0f;
        public float deltaSpeed = 0.2f;

        public SdnFolder[] folders;
        public GameObject[] pivots;
        public GameObject[] children;

        private UnitySpriterAnimator animator;
        private SpriterEntity entity;

        [HideInInspector]
        public string spriterData;

        public void Awake()
        {
            Spriter spriter = Spriter.Parse(spriterData);
            entity = spriter.Entities[0];
            animator = new UnitySpriterAnimator(entity, pivots, children);
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
            animator.Speed = animatorSpeed;
            animator.Step(Time.deltaTime * 1000.0f);

            if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") > 0)
                ChangeAnimation(1);
            if (Input.GetButtonDown("Horizontal") && Input.GetAxis("Horizontal") < 0)
                ChangeAnimation(-1);
            if (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") > 0)
                ChangeAnimationSpeed(deltaSpeed);
            if (Input.GetButtonDown("Vertical") && Input.GetAxis("Vertical") < 0)
                ChangeAnimationSpeed(-deltaSpeed);
            if (Input.GetButtonDown("Jump"))
                ReverseAnimation();
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
            speed = Math.Abs(speed) < maxSpeed ? speed : maxSpeed * Math.Sign(speed);
            animatorSpeed = (float)Math.Round(speed, 1, MidpointRounding.AwayFromZero);
        }

        private void ReverseAnimation()
        {
            animatorSpeed *= -1;
        }

        private void RegisterSprites()
        {
            for (int i = 0; i < folders.Length; ++i)
            {
                Sprite[] files = folders[i].Files;
                for (int j = 0; j < files.Length; ++j)
                {
                    animator.Register(i, j, files[j]);
                }
            }
        }
    }

}
