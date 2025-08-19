using System.Collections.Generic;
using UnityEngine;

public class RocketPoolManager : MonoBehaviour
{
    public static RocketPoolManager Instance { get; private set; }

    [SerializeField] private Rocket rocketPrefab;
    [SerializeField] private int poolSize = 10;

    private Queue<Rocket> _rocketPool = new Queue<Rocket>();

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < poolSize; i++)
        {
            Rocket newRocket = Instantiate(rocketPrefab, transform);
            newRocket.gameObject.SetActive(false);
            _rocketPool.Enqueue(newRocket);
        }
    }

    public Rocket GetRocket()
    {
        if (_rocketPool.Count > 0)
        {
            Rocket rocket = _rocketPool.Dequeue();
            rocket.gameObject.SetActive(true);
            return rocket;
        }
        
        return Instantiate(rocketPrefab, transform);
    }

    public void ReturnRocket(Rocket rocket)
    {
        rocket.gameObject.SetActive(false);
        _rocketPool.Enqueue(rocket);
    }
}