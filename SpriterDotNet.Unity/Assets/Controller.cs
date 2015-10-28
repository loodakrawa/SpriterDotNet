using UnityEngine;
using SpriterDotNetUnity;
using System;
using System.Linq;
using System.Collections.Generic;

public class Controller : MonoBehaviour
{
    public float MaxSpeed = 5.0f;
    public float DeltaSpeed = 0.2f;
    public float TransitionTime = 1.0f;

    [HideInInspector]
    public float AnimatorSpeed = 1.0f;

    private IList<UnitySpriterAnimator> animators;

    void Update()
    {
        if(animators == null) animators = FindObjectsOfType<SpriterDotNetBehaviour>().Select(x => x.Animator).ToList();

        if (GetAxisDownPositive("Horizontal")) SwitchAnimation(1);
        if (GetAxisDownNegative("Horizontal")) SwitchAnimation(-1);
        if (GetAxisDownPositive("Vertical")) ChangeAnimationSpeed(DeltaSpeed);
        if (GetAxisDownNegative("Vertical")) ChangeAnimationSpeed(-DeltaSpeed);
        if (Input.GetButtonDown("Jump")) ReverseAnimation();
        if (Input.GetButtonDown("Fire1")) Transition(1);

        foreach(var animator in animators) animator.Speed = AnimatorSpeed;
    }

    private void SwitchAnimation(int offset)
    {
        foreach (var animator in animators)
        {
            animator.Play(GetAnimation(animator, offset));
        }
    }

    private void Transition(int offset)
    {
        foreach (var animator in animators) animator.Transition(GetAnimation(animator, offset), TransitionTime * 1000.0f);
    }

    private void ChangeAnimationSpeed(float delta)
    {
        foreach(var animator in animators)
        {
            var speed = animator.Speed + delta;
            speed = Math.Abs(speed) < MaxSpeed ? speed : MaxSpeed * Math.Sign(speed);
            AnimatorSpeed = (float)Math.Round(speed, 1, MidpointRounding.AwayFromZero);
        }
    }

    private void ReverseAnimation()
    {
        AnimatorSpeed *= -1;
    }

    private static bool GetAxisDownPositive(string axisName)
    {
        return Input.GetButtonDown(axisName) && Input.GetAxis(axisName) > 0;
    }

    private static bool GetAxisDownNegative(string axisName)
    {
        return Input.GetButtonDown(axisName) && Input.GetAxis(axisName) < 0;
    }

    private static string GetAnimation(UnitySpriterAnimator animator, int offset)
    {
        List<string> animations = animator.GetAnimations().ToList();
        int index = animations.IndexOf(animator.CurrentAnimation.Name);
        index += offset;
        if (index >= animations.Count) index = 0;
        if (index < 0) index = animations.Count - 1;
        return animations[index];
    }
}
