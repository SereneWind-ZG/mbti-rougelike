﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有实体的基类，包含单位，建筑，敌人，玩家等子类共有的一些参数和方法。
/// </summary>
public abstract class BaseEntity : MonoBehaviour, IEntity
{
    [Header("实体数据")]
    [SerializeField, Tooltip("该实体的当前生命。")]
    protected int hp;

    [SerializeField, Tooltip("该实体的生命上限。")]
    protected int maxHp;

    [SerializeField, Tooltip("该实体的生命再生。")]
    protected int hpRegen;

    [SerializeField, Tooltip("当前实体的【速度】向量值")]
    protected Vector3 velocity;

    [SerializeField, Tooltip("当前实体的【吹飞速度】向量值")]
    protected Vector3 blowForceVelocity;

    [SerializeField, Tooltip("当前实体的【移动速度】。")]
    protected float movementSpeed = 1.0f;

    [SerializeField, Tooltip("当速度低于此阈值时，将【吹飞速度】清零。")]
    protected float stopBlowThreadshold = 0.5f;

    [SerializeField, Tooltip("此数值决定了【吹飞速度】的下降率")]
    protected float blowSpeedReduceRate = 0.1f;

    [SerializeField, Tooltip("该实体的当前护盾。")]
    protected int shield;

    [SerializeField, Tooltip("该实体的护盾上限。")]
    protected int maxShield;

    [SerializeField, Tooltip("该实体的护盾再生。")]
    protected int shieldRegen;


    [Header("互动组件")]
    public HPController hpControllerPrefab;
    public Transform canvasTransform;

    private Dictionary<GameObject, float> damageTimers = new Dictionary<GameObject, float>();

    public event Action OnDeath;

    private Coroutine hpRegenCoroutine;
    private Coroutine shieldRestoreCoroutine;

    protected virtual void Start()
    {
        canvasTransform = GameObject.Find("Canvas").transform;
        var healthBarInstance = Instantiate(hpControllerPrefab, canvasTransform);
        healthBarInstance.baseEntity = this;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + healthBarInstance.offset);
        healthBarInstance.GetComponent<RectTransform>().position = screenPosition;

        hp = maxHp;
        shield = maxShield;

        StartHealthRegen();

        shieldRestoreCoroutine = StartCoroutine(ShieldRestoreRoutine());
    }

    void Update()
    {
        velocity = new Vector3(velocity.x, velocity.y, 0.0f);
        OnUpdate();

        List<GameObject> keys = new List<GameObject>(damageTimers.Keys);
        foreach (var key in keys)
        {
            damageTimers[key] -= Time.deltaTime;
            if (damageTimers[key] <= 0)
            {
                damageTimers.Remove(key);
            }
        }
    }

    protected virtual void OnUpdate()
    {}

    void FixedUpdate()
    {
        transform.Translate(velocity * Time.fixedDeltaTime);
        transform.Translate(blowForceVelocity * Time.fixedDeltaTime);

        blowForceVelocity = new Vector3
(blowSpeedReduceUpdate(blowForceVelocity.x), blowSpeedReduceUpdate(blowForceVelocity.y), 0.0f);

        if (Mathf.Abs(blowForceVelocity.x) < stopBlowThreadshold)
            blowForceVelocity = new Vector3(0.0f, blowForceVelocity.y, 0.0f);
        if (Mathf.Abs(blowForceVelocity.y) < stopBlowThreadshold)
            blowForceVelocity = new Vector3(blowForceVelocity.x, 0.0f, 0.0f);

    }

    public int HP
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
        }
    }

    public int MaxHP
    {
        get
        {
            return maxHp;
        }
        set
        {
            maxHp = value;
        }
    }

    public int Shield
    {
        get
        {
            return shield;
        }
        set
        {
            shield = value;
        }
    }

    public int MaxShield
    {
        get
        {
            return maxShield;
        }
        set
        {
            maxShield = value;
        }
    }

    public Vector3 Velocity
    {
        get
        {
            return velocity;
        }
        set
        {
            velocity = value;
        }
    }

    public Vector3 BlowForceVelocity
    {
        get
        {
            return blowForceVelocity;
        }
        set
        {
            blowForceVelocity = value;
        }
    }

    public virtual void TakeDamage(int damage, float stuntime)
    {
        ResetShieldRestoreCoroutine();

        if (shield > 0)
        {
            shield -= damage;
            if (shield < 0)
            {
                // 将穿透护盾的伤害施加到声明上。
                hp += shield;
            }
        }
        else
        {
            hp -= damage;
        }

        if (hp <= 0)
        {
            Die();
        }
        else if (hp > maxHp)
        {
            hp = maxHp;
        }
    }

    protected virtual void Die()
    {
        //Destroy(gameObject); // for now
        gameObject.SetActive(false);
        OnDeath?.Invoke();
    }

    public float blowSpeedReduceUpdate(float speed)
    {
        if (speed > 0)
        {
            speed -= blowSpeedReduceRate;
            if (speed < 0) speed = 0;
        }
        else
        {
            speed += blowSpeedReduceRate;
            if (speed > 0) speed = 0;
        }

        return speed;
    }

    public virtual bool CanTakeDamageFrom(GameObject collider)
    {
        return !damageTimers.ContainsKey(collider);
    }

    public void SetDamageTimer(GameObject collider, float timer)
    {
        damageTimers[collider] = timer;
    }

    private void StartHealthRegen()
    {
        if (hpRegenCoroutine != null)
        {
            StopCoroutine(hpRegenCoroutine);
        }
        hpRegenCoroutine = StartCoroutine(HpRegenHealthOverTime());
    }

    private IEnumerator HpRegenHealthOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            if (hp < maxHp)
            {
                hp += hpRegen;
                hp = Mathf.Min(hp, maxHp);
            }
        }
    }

    private IEnumerator ShieldRestoreRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);
            shield = maxShield;
        }
    }

    private void ResetShieldRestoreCoroutine()
    {
        if (shieldRestoreCoroutine != null)
        {
            StopCoroutine(shieldRestoreCoroutine);
        }
        shieldRestoreCoroutine = StartCoroutine(ShieldRestoreRoutine());
    }
}