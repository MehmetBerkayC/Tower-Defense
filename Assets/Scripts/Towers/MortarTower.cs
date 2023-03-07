using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MortarTower : Tower
{

    [SerializeField, Range(0.5f, 2f)] 
    float shotsPerSecond = 1f;
    
    [SerializeField, Range(0.5f, 3f)] 
    float shellBlastRadius = 1f;
    
    [SerializeField, Range(1f, 100f)]
    float shellDamage = 10f;
    
    [SerializeField]
    Transform mortar = default;
    public override TowerType TowerType => TowerType.Mortar;

    float launchSpeed, launchProgress;

	void Awake () {
		OnValidate();
	}

	void OnValidate () {
		float x = targetingRange + 0.25001f;
		float y = -mortar.position.y;
		launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
	}

	public override void GameUpdate () {
		launchProgress += shotsPerSecond * Time.deltaTime;
		while (launchProgress >= 1f) {
			if (AcquireTarget(out TargetPoint target)) {
				Launch(target);
				launchProgress -= 1f;
			}
			else {
				launchProgress = 0.999f;
			}
		}
	}
    private void Launch(TargetPoint target)
    {
        Vector3 launchPoint = mortar.position;
        Vector3 targetPoint = target.Position;

        targetPoint.y = 0f;

        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;

        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x; // Normalizing

        // Magic -> https://catlikecoding.com/unity/tutorials/tower-defense/ballistics/#:~:text=types%2C%20one%20inactive-,Calculating%20Trajectories,-A%20mortar%20works
        // tan() = ( s^2 + sqrroot( s^4 - g(gx^2 + 2ys^2) ) ) / gx 
       
        float g = 9.81f; // Gravity
        float s = launchSpeed; 
        float s2 = s * s;

        float r = s2 * s2 - g * (g * x * x + 2f * y * s2);
        
        Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");
        
        if(r < 0f) // DO NOT CHANGE UNDER ANY CIRCUMSTANCE!
        {
            return;
        }

        float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
        float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
        float sinTheta = cosTheta * tanTheta;

        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));

        Game.SpawnShell().Initialize(
            launchPoint, targetPoint, 
            new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y),
            shellDamage, shellBlastRadius
            );


        //// Visualize first second of the flight by ten segment lines
        //Vector3 prev = launchPoint, next;
        //for (int i = 1; i <= 10; i++)
        //{
        //    float t = i / 10f;
        //    float dx = s * cosTheta * t;
        //    float dy = s * sinTheta * t - 0.5f * g * t * t;
        //    next = launchPoint + new Vector3(dir.x * dx, dy, dir.y * dx);
        //    Debug.DrawLine(prev, next, Color.blue, 1f);
        //    prev = next;
        //}

        //Debug.DrawLine(launchPoint, targetPoint, Color.yellow, 1f);
        //Debug.DrawLine(
        //    new Vector3(launchPoint.x, 0.01f, launchPoint.z),
        //    new Vector3(launchPoint.x + dir.x * x, 0.01f, launchPoint.z + dir.y * x),
        //    Color.white, 1f
        //    );

    }


}
