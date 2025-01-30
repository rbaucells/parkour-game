using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AnimationScript : MonoBehaviour
{
    public Animator animator;

    /// <summary>
    /// 0 - Very Small
    /// 1 - Small
    /// 2 - Medium
    /// 3 - Big
    /// 4 - Very Big
    /// 5 - Very Very Big
    /// </summary>
    /// <param name="size"></param>
    public void Recoil(int index, int size)
    {
        switch (size)
        {
            case 0:
                Play("Very Small", ProcessIndex(index));
                Debug.Log("Very Small Recoil");
                break;

            case 1:
                Play("Small", ProcessIndex(index));
                Debug.Log("Small Recoil");
                break;

            case 2:
                Play("Medium", ProcessIndex(index));
                Debug.Log("Medium Recoil");
                break;

            case 3:
                Play("Big", ProcessIndex(index));
                Debug.Log("Big Recoil");
                break;

            case 4:
                Play("Very Big", ProcessIndex(index));
                Debug.Log("Very Big Recoil");
                break;

            case 5:
                Play("Very Very Big", ProcessIndex(index));
                Debug.Log("Very Very Big Recoil");
                break;
        }
    }

    string ProcessIndex(int index) 
    {
        switch (index)
        {
            case 0:
                return "GunPos 0 Recoil Layer";
            case 1:
                return "GunPos 1 Recoil Layer";
            case 2:
                return "GunPos 2 Recoil Layer";
            case 3:
                return "GunPos 3 Recoil Layer";
            default:
                return null;
        }
    }

    /// <summary>
    /// 0 - Right in
    /// 1 - Left in
    /// </summary>
    /// <param name="side"></param>
    public void WallRunIn(int side)
    {
        switch (side)
        {
            case 0:
                Play("Right In", "Wall Running Layer");
                break;

            case 1:
                Play("Left In", "Wall Running Layer");
                break;
        }
    }

    /// <summary>
    /// 0 - Right out
    /// 1 - Left out
    /// </summary>
    /// <param name="side"></param>
    public void WallRunOut(int side)
    {
        switch (side)
        {
            case 0:
                animator.SetBool("Right Out", true);
                StartCoroutine(TurnOffWallRunOutBool());
                break;

            case 1:
                animator.SetBool("Left Out", true);
                StartCoroutine(TurnOffWallRunOutBool());
                break;
        }
    }
    /// <summary>
    /// 0 - Dash Forward
    /// 1 - Dash Back
    /// 2 - Dash Right
    /// 3 - Dash Left
    /// </summary>
    /// <param name="side"></param>
    public void Dash(int side)
    {
        switch (side)
        {
            case 0:
                Play("Dash Forward", "Dash Forward/Back Layer");
                break;
            case 1:
                Play("Dash Back", "Dash Forward/Back Layer");
                break;
            case 2:
                Play("Dash Right", "Dash Right/Left Layer");
                break;
            case 3:
                Play("Dash Left", "Dash Right/Left Layer");
                break;
        }
    }

    public void Jump()
    {
        Debug.Log("Jump");
        Play("Jump", "Jump Layer");
    }

    public void Land()
    {
        Debug.Log("Land");
        Play("Land", "Land Layer");        
    }

    /// <summary>
    /// 0 - Slam Left
    /// 1 - Slam Middle
    /// 2 - Slam Right
    /// </summary>
    /// <param name="side"></param>
    public void Slam(int side)
    {
        switch (side)
        {
            case 0:
                Play("Ground Slam Left", "Slam Layer");
                Debug.Log("Slam Left");
                break;
            case 1:
                Play("Ground Slam Middle", "Slam Layer");
                Debug.Log("Slam Middle");
                break;
            case 2:
                Play("Ground Slam Right", "Slam Layer");
                Debug.Log("Slam Right");
                break;
        }
    }

    public void StartWalk()
    {
        Debug.Log("Start Walk");
        Play("Walk", "Walk Layer");
    }

    public void StopWalk()
    {
        Debug.Log("Stop Walk");
        animator.SetBool("stopWalking", false);
        StartCoroutine(TurnOffWalkingBool());
    }

    IEnumerator TurnOffWalkingBool()
    {
        yield return null;
        animator.SetBool("stopWalking", true);
    }

    IEnumerator TurnOffWallRunOutBool()
    {
        yield return null;
        animator.SetBool("Right Out", false);
        animator.SetBool("Left Out", false);
    }

    private int GetLayerIndex(string name)
    {
        return animator.GetLayerIndex(name);
    }

    private void Play(string animName, string layerName)
    {
        if (layerName == null)
        {
            Debug.LogError("Layer name is null");
            return;
        }
        Debug.Log("Playing " + animName + " on " + layerName);
        animator.Play(animName, GetLayerIndex(layerName), 0.0f);
    }
}
