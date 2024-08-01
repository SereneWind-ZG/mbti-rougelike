using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewENTPStatus", menuName = "Status Data/ENTP Ult Data")]
public class StatusEntpUlt : Status
{
    public float attackSpeedBonus;
    public float attackRangedBonus;

    public Stats stats;

    public override void OnUpdate(GameObject target, float deltaTime)
    {
        base.OnUpdate(target, deltaTime);
    }

    public override void OnApply(GameObject target)
    {
       base.OnApply(target);

       stats = target.GetComponent<Player>().stats;
       target.GetComponent<Player>().stats.attackSpeed += attackSpeedBonus;
       target.GetComponent<Player>().stats.attackRange += attackRangedBonus;
    }

    public override void OnExpire(GameObject target)
    {
        base.OnExpire(target);

        target.GetComponent<Player>().stats.attackSpeed -= attackSpeedBonus;
        target.GetComponent<Player>().stats.attackRange -= attackRangedBonus;

    }

}
