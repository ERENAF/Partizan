using System.Collections.Generic;
using UnityEngine;

public interface IPausable{};

public class GamePauseManager : MonoBehaviour
{
    public static GamePauseManager Instance {get;private set;}
    public bool IsPaused{get;private set;}

    private readonly List<Behaviour> _pasueables = new();

    private void Awake()
    {
        if (Instance != null) {Destroy(gameObject);return;}
        Instance = this;
    }
    private void Start()
    {
        var all = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        foreach (var mb in all)
            if (mb is IPausable) _pasueables.Add(mb);
    }

    public void SetPause(bool pause)
    {
        if (IsPaused == pause)return;

        IsPaused = pause;
        Time.timeScale = pause ? 0f : 1f;

        for (int i = 0; i < _pasueables.Count; i++)
        {
            if (_pasueables[i] != null) _pasueables[i].enabled = !pause;
        }
    }
}
