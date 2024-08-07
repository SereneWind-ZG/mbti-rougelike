﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用来控制所有2D帧动画的类。
/// </summary>
public class AnimationController2D : MonoBehaviour, IPoolable
{
    protected Animator animator;
    public bool isAttached = false;
    public Transform attachedTransform;
    public bool animationFinished = false;
    public string poolKey;

    public Animator GetAnimator()
    {
        return animator;
    }

    public string PoolKey
    {
        get { return poolKey; }
        set { poolKey = value; }
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        poolKey = gameObject.name;
    }

    private void Update()
    {
        if (isAttached)
        {
            transform.position = attachedTransform.position;
        }
    }

    public void PlayAnimation(string name)
    {
        animator.Play(name);
    }

    public void OnAnimationEnd()
    {
        animationFinished = true;
        gameObject.SetActive(false);
        //var damageCollider = gameObject.GetComponent<DamageCollider>();
        //if (damageCollider)
        //{
        //    damageCollider.Deactivate();
        //}
        //Destroy(gameObject);
    }

    /// <summary>
    /// 继承自IPoolable接口的方法。用于对象池物体的初始化。
    /// </summary>
    public void ResetObjectState()
    {
        animationFinished = false;
    }

    /// <summary>
    /// 当对象从对象池中取出时，调用这个方法来初始化
    /// </summary>
    public void Activate(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 调用这个方法将对象塞回对象池
    /// </summary>
    public void Deactivate()
    {
        gameObject.SetActive(false);

        PoolManager.Instance.ReturnObject(poolKey, gameObject);
    }
}
