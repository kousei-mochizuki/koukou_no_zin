using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public Transform firePoint;
    public float slashSpeed = 10f;
    public float minAttackInterval = 0.5f;
    public float maxAttackInterval = 3f;
    public float minDistance = 1f;
    public float maxDistance = 10f;
    //public float slashLifetime = 5f;
    public Transform enemyModel;

    [SerializeField] private float currentAttackInterval;
    private Transform player;
    private float attackTimer;
    private Vector3 directionToPlayer;
    private Quaternion rotationToPlayer;

    [System.Serializable]
    public class AttackData
    {
        public GameObject attackPrefab;
        public Vector3 angle;
        public bool isSkiping;
        public float delay;
        public float speed;
        public float lifetime;
    }

    [System.Serializable]
    public class AttackPattern
    {
        public List<AttackData> attacks;
    }

    public List<AttackPattern> attackPatterns;
    public bool loopAttackPatterns = true;

    private Dictionary<GameObject, Queue<GameObject>> attackPools = new Dictionary<GameObject, Queue<GameObject>>();
    private int currentPatternIndex = 0;

    void Start()
    {
        player = Camera.main.transform;
        InitializePools();
    }

    void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(firePoint.position, player.position);

        float t = Mathf.InverseLerp(minDistance, maxDistance, distanceToPlayer);
        currentAttackInterval = Mathf.SmoothStep(maxAttackInterval, minAttackInterval, t);

        attackTimer -= Time.deltaTime;

        directionToPlayer = player.position - firePoint.position;
        rotationToPlayer = Quaternion.LookRotation(directionToPlayer);
        enemyModel.rotation = rotationToPlayer;

        if (attackTimer <= 0f)
        {
            StartCoroutine(ExecuteAttackPattern());
            attackTimer = currentAttackInterval;
        }
    }

    IEnumerator ExecuteAttackPattern()
    {
        AttackPattern currentPattern = attackPatterns[currentPatternIndex];

        foreach (AttackData attackData in currentPattern.attacks)
        {
            StartCoroutine(FireAttackWithDelay(attackData));
        }

        currentPatternIndex++;

        if (currentPatternIndex >= attackPatterns.Count)
        {
            currentPatternIndex = loopAttackPatterns ? 0 : attackPatterns.Count - 1;
        }

        yield return null;
    }

    IEnumerator FireAttackWithDelay(AttackData attackData)
    {
        yield return new WaitForSeconds(attackData.delay);

        if (attackData.isSkiping)
        {
            yield break; // スキップ
        }

        FireAttack(attackData);
    }

    void FireAttack(AttackData attackData)
    {
        GameObject attack = GetAttackFromPool(attackData.attackPrefab);
        if (attack == null)
        {
            Debug.LogError("Attack object is null! Make sure attackPrefab is assigned.");
            return;
        }

        attack.transform.position = firePoint.position;

        Quaternion customRotation = Quaternion.Euler(rotationToPlayer.eulerAngles.x + attackData.angle.x,
                                                    rotationToPlayer.eulerAngles.y + attackData.angle.y,
                                                    attackData.angle.z);

        attack.transform.rotation = customRotation;
        attack.SetActive(true);

        Projectile projectileScript = attack.GetComponent<Projectile>();
        if (projectileScript == null)
        {
            projectileScript = attack.AddComponent<Projectile>();
        }

        projectileScript.Initialize(attackData.speed, attackData.lifetime, player);
    }

    void InitializePools()
    {
        foreach (AttackPattern pattern in attackPatterns)
        {
            foreach (AttackData attackData in pattern.attacks)
            {
                if (!attackPools.ContainsKey(attackData.attackPrefab))
                {
                    attackPools[attackData.attackPrefab] = new Queue<GameObject>();
                    for (int i = 0; i < 10; i++) // Adjust initial pool size as needed
                    {
                        GameObject attack = Instantiate(attackData.attackPrefab);
                        attack.SetActive(false);
                        attackPools[attackData.attackPrefab].Enqueue(attack);
                    }
                }
            }
        }
    }

    GameObject GetAttackFromPool(GameObject prefab)
    {
        if (attackPools.ContainsKey(prefab) && attackPools[prefab].Count > 0)
        {
            return attackPools[prefab].Dequeue();
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    public void ReturnAttackToPool(GameObject attack, GameObject prefab)
    {
        attack.SetActive(false);
        if (!attackPools.ContainsKey(prefab))
        {
            attackPools[prefab] = new Queue<GameObject>();
        }
        attackPools[prefab].Enqueue(attack);
    }
}

public class Projectile : MonoBehaviour
{
    private float speed;
    private float lifetime;
    private Transform player;
    private Vector3 direction;
    private EnemyAttack enemyAttack;

    public void Initialize(float speed, float lifetime, Transform player)
    {
        this.speed = speed;
        this.lifetime = lifetime;
        this.player = player;
        direction = transform.forward;
        enemyAttack = FindObjectOfType<EnemyAttack>();
        Invoke("ReturnToPool", lifetime);
    }

    void FixedUpdate()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void ReturnToPool()
    {
        enemyAttack.ReturnAttackToPool(gameObject, gameObject);
    }
}
