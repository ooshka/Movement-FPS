using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private Enemy enemyPrefab;

    [Header("Starting Variables")]
    [SerializeField]
    private Vector3 enemyStartingPosition;
    [SerializeField]
    private Quaternion enemyRotation;
    [SerializeField]
    private int numEnemies;
    [SerializeField]
    private float spacing;
    [SerializeField]
    private Vector3 lineDirection;

    [Header("Movement Variables")]
    [SerializeField]
    private float moveSpeed;
    private int moveDirectionMagnitude = 1;


    List<EnemyHandler> enemyHandles = new List<EnemyHandler>();
    Timer moveDirectionTimer = new Timer(3f, false);

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            Vector3 position = enemyStartingPosition + spacing * i * lineDirection;
            enemyHandles.Add(GetEnemyHandler(position));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(moveDirectionTimer.CanTriggerEventAndReset())
        {
            moveDirectionMagnitude *= -1;
        }

        Move();

        moveDirectionTimer.Iterate(Time.deltaTime);
    }

    private void Move()
    {
        Vector3 moveAmount = moveSpeed * moveDirectionMagnitude * lineDirection * Time.deltaTime;
        foreach(EnemyHandler enemyHandler in enemyHandles)
        {
            if (enemyHandler.enemy == null)
            {
                enemyHandler.position += moveAmount;
                enemyHandler.IterateTimer(Time.deltaTime);
                if (enemyHandler.CanRespawn())
                {
                    enemyHandler.enemy = Instantiate(enemyPrefab, enemyHandler.position, enemyRotation, transform);
                    enemyHandler.ResetTimer();
                }
            } else
            {
                enemyHandler.enemy.transform.Translate(enemyHandler.enemy.transform.InverseTransformDirection(moveAmount));
                enemyHandler.position = enemyHandler.enemy.transform.position;
            }
        }
    }

    private EnemyHandler GetEnemyHandler(Vector3 position)
    {
        return new EnemyHandler(position, Instantiate(enemyPrefab, position, enemyRotation, transform));
    }
}

public class EnemyHandler
{
    public Enemy enemy;
    public Vector3 position;
    private float respawnTime = 2f;
    private float timer;

    public EnemyHandler(Vector3 position, Enemy enemy)
    {
        this.position = position;
        this.enemy = enemy;
        timer = respawnTime;
    }

    public void IterateTimer(float timeInterval)
    {
        timer -= timeInterval;
    }

    public bool CanRespawn()
    {
        return timer <= 0;
    }

    public void ResetTimer()
    {
        timer = respawnTime;
    }
}
