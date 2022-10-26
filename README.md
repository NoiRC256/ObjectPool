# ObjectPool
A clean, flexible generic object pooling solution for Unity.

## Key Features

:star: Supports prefab pooling with generic types, no `GetComponent<>` calls needed

:star: Dynamic expansion with configurable capacities

:star: High flexibility, pooling behaviour defined by constructors

:star: Poolable types don't have to implement some specific interface. This allows easier and cleaner integration


## Usage Example

The pool user creates a pool and defines its behaviour on various pooling stages. When needed, pooled instances are taken from the pool to be used.

```csharp
// Object pool user.
public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private Projectile _prefab;
    [SerializeField] private Transform _muzzle;
    private NekoNeko.ObjectPool<Projectile> ProjectilePool {get; set;}

    private void Awake()
    {
        // Initialize pool, pass in delegates to define pooling behaviour.
        ProjectilePool = ObjectPool<Projectile>.Create(_prefab,
            CreateProjectile, OnTakeProjectile, OnReleaseProjectile, OnDestroyProjectile);
    }

    public void Shoot()
    {
        // Take pooled instance from pool and initialize it.
        Projectile p = ProjectilePool.Take();
        p.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);
        p.SetActive(true);
    }

    // This will be called when the pool runs out of pooled instances and performs a refill.
    private Projectile CreateProjectile()
    {
        return Instantiate(_prefab);
    }

    // This will be called when a pooled instance is taken from the pool.
    private void OnTakeProjectile(Projectile p)
    {
        p.Reset();
        p.ObjectPool = p;
    }

    // This will be called when a pooled instance is released back into a pool.
    private void OnReleaseProjectile(Projectile p)
    {
         p.gameObject.SetActive(false);
    }

    // This will be called when a pooled instance must be destroyed...
    // ...usually because the pool has reached max capacity.
    private void OnDestroyProjectile(Projectile p)
    {
        Destroy(p);
    }
}
```

Each pooled instance maintains a reference to its source pool, and is responsible for releasing itself back into the pool.
```csharp
// A poolable type.
public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 0f;
    [SerializeField] private float _aliveDuration = 10f;
    private float _timeElapsed = 0f;

    // Reference of source pool.
    public NekoNeko.ObjectPool<Projectile> ObjectPool { get; set; }

    public void Reset()
    {
        _speed = 0f;
        _aliveDuration = 10f;
        _timeElapsed = 0f;
    }

    private void FixedUpdate()
    {
        _timeElapsed += Time.deltaTime;
        if(_timeElapsed >= _aliveDuration) Remove();
        Vector3 vel = _speed * Time.deltaTime * transform.forward;
        transform.position += vel;
    }

    public void Remove()
    {
        if(ObjectPool != null) ObjectPool.Release(this);
        else Destroy(this.gameObject);
    }
}

```
