using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : GameBehavior
{
    EnemyFactory originFactory;

    GameTile tileFrom, tileTo;
    Vector3 positionFrom, positionTo;
    float progress, progressFactor;

    [SerializeField] Transform model = default;

    // Direction 
    Direction direction;
    DirectionChange directionChange;
    float directionAngleFrom, directionAngleTo;

    float pathOffset;
    float speed;

    [SerializeField]
    EnemyAnimationConfig animationConfig = default;

    EnemyAnimator animator;

    Collider targetPointCollider;

    public Collider TargetPointCollider
    {
        set
        {
            Debug.Assert(targetPointCollider == null, "Redefined collider!");
            targetPointCollider = value;
        }
    }

    public bool IsValidTarget => animator.CurrentClip == EnemyAnimator.Clip.Move;

    float Health { get; set; }

    public float Scale { get; private set; }

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    private void Awake()
    {
        animator.Configure(
            model.GetChild(0).gameObject.AddComponent<Animator>(), 
            animationConfig);

        targetPointCollider.enabled = false; // Will disable collider if immobile

    }

    public void Initialize(float scale, float speed, float pathOffset, float health)
    {
        Scale = scale;
        model.localScale = new Vector3(scale, scale, scale);
        this.speed = speed;
        this.pathOffset = pathOffset;
        Health = health;

        animator.PlayIntro();
    }

    private void OnDestroy()
    {
        animator.Destroy();    
    }

    public void SpawnOn(GameTile tile)
    {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        
        progress = 0f;
        PrepareIntro();
    }

    public override bool GameUpdate()
    {
        animator.GameUpdate();
        if(animator.CurrentClip == EnemyAnimator.Clip.Intro)
        {
            if (!animator.IsDone)
            {
                return true; 
            }
            animator.PlayMove(speed / Scale);
            targetPointCollider.enabled = true;
        }
        else if(animator.CurrentClip >= EnemyAnimator.Clip.Outro) // outro or dying
        {
            if (animator.IsDone)
            {
                Recycle();
                return false;
            }
            return true;
        }

        if(Health <= 0f)
        {
            animator.PlayDying();
            targetPointCollider.enabled = false;
            return true;
        }

        progress += Time.deltaTime * progressFactor;
        while (progress >= 1)
        {
            if (tileTo == null) // in destination - nowhere to go
            {
                Game.EnemyReachedDestination();
                animator.PlayOutro();
                return true;
            }

            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }

        if(directionChange == DirectionChange.None)
        {
            transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }
        else
        {
            float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
    }

    public override void Recycle()
    {
        animator.Stop();
        OriginFactory.Reclaim(this);
    }

    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative Damage Applied!");
        Health -= damage;
    }

    private void PrepareNextState()
    {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;

        positionFrom = positionTo;

        if (tileTo == null)
        {
            PrepareOutro();
            return;
        }

        positionTo = tileFrom.ExitPoint;

        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;

        switch (directionChange)
        {
            case DirectionChange.None:
                PrepareForward();
                break;
            case DirectionChange.TurnRight:
                PrepareTurnRight();
                break;
            case DirectionChange.TurnLeft:
                PrepareTurnLeft();
                break;
            default:
                PrepareTurnAround();
                break;
        }
    }

    private void PrepareForward()
    {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();

        model.localPosition = new Vector3(pathOffset, 0f);

        progressFactor = speed;
    }

    private void PrepareTurnRight()
    {
        directionAngleTo = directionAngleFrom + 90f;

        model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
    }

    private void PrepareTurnLeft()
    {
        directionAngleTo = directionAngleFrom - 90f;

        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
    }

    void PrepareTurnAround()
    {
        directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);

        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localPosition = positionFrom;
        
        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
    }

    private void PrepareIntro()
    {
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom; // For the animations
        positionTo = tileFrom.ExitPoint; // The edge we exit the tile

        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = directionAngleTo = direction.GetAngle();

        model.transform.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }

    private void PrepareOutro()
    {
        positionTo = tileFrom.transform.localPosition;

        directionChange = DirectionChange.None;
        directionAngleTo = directionAngleFrom;
        
        model.localPosition = new Vector3(pathOffset, 0f);
        
        transform.localRotation = direction.GetRotation();
        
        progressFactor = 2F * speed;
    }
}

