using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : Tower
{
    TargetPoint target;

    [SerializeField] 
    Transform turret = default, laserBeam = default;
    
    private Vector3 laserBeamScale;

    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;

    public override TowerType TowerType => TowerType.Laser;

    private void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    public override void GameUpdate()
    {
        if (TrackTarget(ref target) || AcquireTarget(out target))
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

}
