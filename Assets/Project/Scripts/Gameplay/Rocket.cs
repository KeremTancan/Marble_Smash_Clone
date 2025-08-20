using UnityEngine;

public class Rocket : MonoBehaviour
{
    private float _speed = 5f;
    private Vector3 _direction;
    private float _lifeTime = 1f;
    private GridManager _gridManager; 
    [SerializeField] private FXManager fxManager;

    public void Launch(Vector3 direction, GridManager gridManager)
    {
        _direction = direction.normalized;
        _gridManager = gridManager;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, _direction); 
        
        StartCoroutine(ReturnToPoolAfterTime(_lifeTime));
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }
    private System.Collections.IEnumerator ReturnToPoolAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        RocketPoolManager.Instance.ReturnRocket(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        Marble marble = other.GetComponent<Marble>();
        if (marble != null)
        {
            if (_gridManager != null)
            {
                _gridManager.ExplodeMarble(marble);
                AudioManager.Instance.PlayExplosionSound();
                fxManager.PlayExplosionEffect(marble.transform.position,marble.MarbleColor);
            }
        }
    }
}