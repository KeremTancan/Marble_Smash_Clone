using System.Collections.Generic;
using UnityEngine;

public class GhostPoolManager : MonoBehaviour
{
    public static GhostPoolManager Instance { get; private set; }

    [SerializeField] private GhostController ghostPrefab;
    [SerializeField] private int poolSize = 3;

    private Queue<GhostController> _ghostPool = new Queue<GhostController>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreatePool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GhostController newGhost = Instantiate(ghostPrefab, transform); 
            newGhost.gameObject.SetActive(false);
            _ghostPool.Enqueue(newGhost);
        }
    }

    public GhostController GetGhost()
    {
        if (_ghostPool.Count > 0)
        {
            GhostController ghost = _ghostPool.Dequeue();
            ghost.gameObject.SetActive(true);
            return ghost;
        }
        
        GhostController newEmergencyGhost = Instantiate(ghostPrefab, transform);
        return newEmergencyGhost;
    }

    public void ReturnGhost(GhostController ghost)
    {
        if (ghost == null) return;
        
        ghost.gameObject.SetActive(false);
        _ghostPool.Enqueue(ghost);
    }
}