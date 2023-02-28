using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : GameTileContent
{
    [SerializeField, Range(1.5f, 10.5f)] float targetingRange = 1.5f;

    TargetPoint target;

    const int enemyLayerMask = 1 << 9;

    static Collider[] targetsBuffer = new Collider[100];

    [SerializeField] 
    Transform turret = default, laserBeam = default;
    
    private Vector3 laserBeamScale;

    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;

    private void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget() || AcquireTarget())
        {
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero; // make laser invisible
        }
    }

    private void Shoot()
    {
        // Turret Rotation
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        // Laser Scaling
        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

        // Apply Damage
        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }

    private bool AcquireTarget()
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 3f;

        int hits = Physics.OverlapCapsuleNonAlloc(a, b, targetingRange, targetsBuffer, enemyLayerMask);
        if(hits > 0)
        {
            target = targetsBuffer[UnityEngine.Random.Range(0,hits)].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
            return true;
        }
        target = null;
        return false;
    }
    bool TrackTarget()
    {
        if (target == null)
        {
            return false;
        }

        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;
        // Why did we calculate distance this way -> https://catlikecoding.com/unity/tutorials/tower-defense/towers/#:~:text=How%20does%20that%20math%20work%3F
        if (x * x + z * z > r * r) // Basically, no square root this way
        {
            target = null;
            return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;

        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);

        if(target != null)
        {
            Gizmos.DrawLine(position, target.Position);
        }
    }
}
