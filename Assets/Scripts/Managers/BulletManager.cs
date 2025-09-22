using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all bullets in the game using Object Pooling for performance
/// Equivalent to bullet vector and Bullet_Move() function from C++
/// </summary>
public class BulletManager : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 100;
    public Transform bulletContainer;
    
    [Header("Bullet Types")]
    public GameObject[] playerBulletPrefabs;
    public GameObject[] enemyBulletPrefabs;
    
    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private List<BulletController> activeBullets = new List<BulletController>();
    
    public static BulletManager Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
        InitializeBulletPool();
    }
    
    void InitializeBulletPool()
    {
        // Create bullet pool for performance
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletContainer);
            bullet.SetActive(false);
            
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletPool.Enqueue(bulletController);
        }
    }
    
    public BulletController SpawnBullet(Vector3 position, Vector3 direction, float speed, int damage, bool isPlayerBullet, BulletType bulletType = BulletType.Normal)
    {
        BulletController bullet = GetPooledBullet();
        if (bullet != null)
        {
            bullet.Initialize(position, direction, speed, damage, isPlayerBullet, bulletType);
            activeBullets.Add(bullet);
            return bullet;
        }
        return null;
    }
    
    BulletController GetPooledBullet()
    {
        if (bulletPool.Count > 0)
        {
            return bulletPool.Dequeue();
        }
        
        // If pool is empty, create new bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletContainer);
        return bullet.GetComponent<BulletController>();
    }
    
    public void ReturnBulletToPool(BulletController bullet)
    {
        if (bullet != null)
        {
            activeBullets.Remove(bullet);
            bullet.gameObject.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }
    
    private void Update()
    {
        // Update all active bullets (equivalent to Bullet_Move() from C++)
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            if (activeBullets[i] == null || !activeBullets[i].gameObject.activeInHierarchy)
            {
                activeBullets.RemoveAt(i);
            }
        }
    }
    
    public void ClearAllBullets()
    {
        foreach (var bullet in activeBullets.ToArray())
        {
            ReturnBulletToPool(bullet);
        }
        activeBullets.Clear();
    }
}

public enum BulletType
{
    Normal,
    Ion,
    Neutron,
    Hyper,
    Vulcan,
    Plasma,
    Laser,
    EnemyEgg
}