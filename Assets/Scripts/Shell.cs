using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : WarEntity
{
    Vector3 launchPoint, targetPoint, launchVelocity;

    float age, damage, blastRadius;

    public void Initialize(
        Vector3 launchPoint, Vector3 targetPoint, 
        Vector3 launchVelocity, float damage, float blastRadius)
    {
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.launchVelocity = launchVelocity;
        this.blastRadius = blastRadius;
        this.damage = damage;
    }

    public override bool GameUpdate()
    {
        // Shell Movement
        age += Time.deltaTime;
        Vector3 p = launchPoint + launchVelocity * age;
        p.y -= 0.5f * 9.81f * age * age;

        if (p.y <= 0f)
        {
            Game.SpawnExplosion().Initialize(targetPoint, blastRadius, damage);

            OriginFactory.Reclaim(this);
            return false;
        }

        transform.localPosition = p;

        // Rotate Shell
        Vector3 d = launchVelocity;
        d.y -= 9.81f * age;
        transform.localRotation = Quaternion.LookRotation(d);

        // Trace
        Game.SpawnExplosion().Initialize(p, 0.1f, 0f); 
        return true;
    }
}
