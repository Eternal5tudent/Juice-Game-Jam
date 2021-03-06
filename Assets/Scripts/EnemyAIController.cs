using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float initPauseTime;
    [SerializeField] private float minX, minY, maxX, maxY;
    [SerializeField] private float minimumTargetDiff = 0.5f;
    [SerializeField] private Transform targetPos;
    private bool waiting;

    [Header("Hub")]
    [SerializeField] private float deathTime;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject hub;
    [SerializeField] private GameObject deathParticles;

    [Header("Other")]
    [SerializeField] private GameObject enemySpawner;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip specialDeathSound;

    private RipplePostProcessor ripplePostProcessor;
    private bool reachedHub;
    private float hubRadius;
    public static Action OnDeath;

    // Start is called before the first frame update
    void Start()
    {
        hubRadius = hub.GetComponent<HubController>().hubRadius;
        hub = GameObject.FindGameObjectWithTag("Hub");
        targetPos.SetParent(null, true);
        targetPos.position = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
        ripplePostProcessor = Camera.main.GetComponent<RipplePostProcessor>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!reachedHub)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos.position, speed * Time.deltaTime);
            if (!waiting)
            {
                if (Vector2.Distance(transform.position, hub.transform.position) <= hubRadius) // Within hub radius
                {
                    TargetHub();
                }
                else if (Vector2.Distance(transform.position, targetPos.position) < minimumTargetDiff) // if we reached our target
                {
                    TargetNewRandomPos();
                }
            }
        }
    }

    private void TargetNewRandomPos()
    {
        IEnumerator TargetNewRandomPos_Cor()
        {
            waiting = true;
            yield return new WaitForSeconds(initPauseTime);
            targetPos.position = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
            waiting = false;
        }
        StartCoroutine(TargetNewRandomPos_Cor());
    }

    private void TargetHub()
    {
        targetPos.position = hub.transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hub"))
        {
            reachedHub = true;
            Attack();
        }
        if (collision.CompareTag("Pilot"))
        {
            Die(true);
        }
    }

    private void Attack()
    {
        IEnumerator Attack_Cor()
        {
            yield return new WaitForSeconds(deathTime);
            hub.GetComponent<HubController>().TakeDamage(damage);
            Die(true);
        }

        StartCoroutine(Attack_Cor());
    }

    public void Die(bool instant = false)
    {
        IEnumerator Die_Cor()
        {
            // TO DO: Play Death Animation, Play Sound Effects, ...
            yield return new WaitForSeconds(deathTime);
            OnDeath?.Invoke();
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
            Destroy(targetPos.gameObject);
            Destroy(this.gameObject);
        }
        if(!instant)
            StartCoroutine(Die_Cor());
        else
        {
            ripplePostProcessor.RippleEffect();
            OnDeath?.Invoke();
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            SFXPlayer.Instance.PlaySFX(deathSound);
            Destroy(targetPos.gameObject);
            Destroy(this.gameObject);
        }
    }

   
}
