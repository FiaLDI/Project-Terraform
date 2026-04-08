using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Linq;

public class AnimationTesterWindow : EditorWindow
{
    private Animator animator;
    private AnimationClip[] clips;

    private PlayableGraph graph;
    private AnimationClipPlayable playable;

    private float speed = 1f;
    private bool loop = true;

    [MenuItem("Tools/Animation Tester")]
    public static void ShowWindow()
    {
        GetWindow<AnimationTesterWindow>("Animation Tester");
    }

    private void OnGUI()
    {
        GUILayout.Label("Animation Tester (Editor Mode)", EditorStyles.boldLabel);

        animator = (Animator)EditorGUILayout.ObjectField("Animator", animator, typeof(Animator), true);

        if (animator == null)
        {
            EditorGUILayout.HelpBox("Assign an Animator", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Load Clips"))
        {
            LoadClips();
        }

        if (clips == null || clips.Length == 0)
        {
            EditorGUILayout.HelpBox("No clips found", MessageType.Warning);
            return;
        }

        speed = EditorGUILayout.Slider("Speed", speed, 0f, 3f);
        loop = EditorGUILayout.Toggle("Loop", loop);

        GUILayout.Space(10);

        foreach (var clip in clips)
        {
            if (GUILayout.Button("▶ " + clip.name))
            {
                PlayClip(clip);
            }
        }

        if (GUILayout.Button("Stop"))
        {
            Stop();
        }
    }

    private void LoadClips()
    {
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("No AnimatorController");
            return;
        }

        clips = animator.runtimeAnimatorController.animationClips
            .Distinct()
            .ToArray();
    }

    private void PlayClip(AnimationClip clip)
    {
        Stop();

        graph = PlayableGraph.Create("AnimationTester");
        graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

        var output = AnimationPlayableOutput.Create(graph, "Animation", animator);

        playable = AnimationClipPlayable.Create(graph, clip);
        playable.SetApplyFootIK(false);
        playable.SetSpeed(speed);

        output.SetSourcePlayable(playable);

        graph.Play();

        EditorApplication.update += UpdateGraph;
    }

    private void UpdateGraph()
    {
        if (!graph.IsValid())
            return;

        float delta = Time.deltaTime;

        graph.Evaluate(delta);

        if (!loop && playable.IsValid())
        {
            if (playable.GetTime() >= playable.GetAnimationClip().length)
            {
                Stop();
            }
        }

        SceneView.RepaintAll();
    }

    private void Stop()
    {
        EditorApplication.update -= UpdateGraph;

        if (graph.IsValid())
        {
            graph.Destroy();
        }
    }

    private void OnDisable()
    {
        Stop();
    }
}
