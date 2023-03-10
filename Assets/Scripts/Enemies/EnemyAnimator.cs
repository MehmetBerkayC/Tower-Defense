using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables; // Playable Graph lib

[System.Serializable]
public struct EnemyAnimator
{
    public enum Clip { Move, Intro, Outro, Dying }

    PlayableGraph graph;

    AnimationMixerPlayable mixer;

    Clip previousClip;

    float transitionProgress;
    const float transitionSpeed = 5f;

    public Clip CurrentClip { get; private set; }

    public bool IsDone => GetPlayable(CurrentClip).IsDone();

    public void Configure(Animator animator, EnemyAnimationConfig config) {
        graph = PlayableGraph.Create();
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, 4);

        var clip = AnimationClipPlayable.Create(graph, config.Move);
        clip.Pause(); // otherwise clip plays in the background
        mixer.ConnectInput((int)Clip.Move, clip, 0);
        
        clip = AnimationClipPlayable.Create(graph, config.Intro);
        clip.SetDuration(config.Intro.length); // To make it play once
        mixer.ConnectInput((int)Clip.Intro, clip, 0);
        
        clip = AnimationClipPlayable.Create(graph, config.Outro);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Outro, clip, 0);

        clip = AnimationClipPlayable.Create(graph, config.Dying);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        mixer.ConnectInput((int)Clip.Dying, clip, 0);

        var output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
        output.SetSourcePlayable(mixer);
    }

    public void PlayIntro()
    {
        SetWeight(Clip.Intro, 1f);
        CurrentClip = Clip.Intro;
        graph.Play();
        transitionProgress = -1f;
    }

    public void PlayMove(float speed)
    {
        GetPlayable(Clip.Move).SetSpeed(speed);
        BeginTransition(Clip.Move);
    }

    public void PlayOutro()
    {
        BeginTransition(Clip.Outro);
    }

    public void PlayDying()
    {
        BeginTransition(Clip.Dying);
    }

    private void BeginTransition(Clip nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    public void GameUpdate()
    {
        if(transitionProgress >= 0f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = -1f; // make progress negative, no transition
                SetWeight(CurrentClip, 1f);
                SetWeight(previousClip, 0f);
                GetPlayable(previousClip).Pause();
            }
            else // Slowly blends them
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1f - transitionProgress);
            }
        }
    }


    Playable GetPlayable(Clip clip)
    {
        return mixer.GetInput((int)clip);
    }


    private void SetWeight(Clip clip, float weight)
    {
        mixer.SetInputWeight((int)clip, weight);
    }

    public void Stop()
    {
        graph.Stop();
    }

    public void Destroy() {
        graph.Destroy();
    }
}