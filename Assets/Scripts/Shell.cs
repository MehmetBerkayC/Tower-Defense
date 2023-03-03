using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : WarEntity
{
    Vector3 launchPoint, targetPoint, launchVelocity;

    float age;

    public void Initialize(Vector3 launchPoint, Vector3 targetPoint, Vector3 launchVelocity)
    {
        this.launchPoint = launchPoint;
        this.targetPoint = targetPoint;
        this.launchVelocity = launchVelocity;
    }

    public override bool GameUpdate()
    {
        // Shell Movement
        age += Time.deltaTime;
        Vector3 p = launchPoint + launchVelocity * age;
        p.y -= 0.5f * 9.81f * age * age;
        transform.localPosition = p;

        // Rotate Shell
        Vector3 d = launchVelocity;
        d.y -= 9.81f * age;
        transform.localRotation = Quaternion.LookRotation(d);
        return true;
    }
}
