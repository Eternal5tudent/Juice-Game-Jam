using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform pilot1;
    [SerializeField] private Transform pilot2;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float maxRotationSpeed;
    [SerializeField] Vector3 minScale;
    [SerializeField] Vector3 maxScale;
    [SerializeField] private float sickomodeTimer;
    [SerializeField] private float maxDistanceBetweenPilots = 3;
    [SerializeField] private float pullPilotSpeed = 7;
    [SerializeField] private float pushPilotSpeed = 10;
    [SerializeField] Cinemachine.CinemachineVirtualCamera virCam;
    [SerializeField] private Animator camAnim;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private float movementForce;
    [SerializeField] private float maxMovementSpeed = 15f;
    [SerializeField] private AudioClip sawSFX;
    [SerializeField] private AudioClip sickomodeSFX;

    public bool takeInput;
    public static Action<Transform> OnSwitch;
    public static Action OnStart;

    private Transform currentPilot;
    private Rigidbody2D rb;
    private Rigidbody2D rb2;
    private AudioSource audioSource;
    private Transform otherPilot;
    private bool pulling;
    private bool startGame;
    private Vector2 distanceBetweenPilots;
    private float rotationRadius;
    private float timer;
    private float timescale;
    private Vector2 movementInput;
    private float msScale;
    private bool sicko;
    public float forceScale = 1; // for better steering while in sicko


    private void Awake()
    {
        EnemyAIController.OnDeath += Screenshake;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        mainMenuUI.SetActive(true);
        startGame = true;
        takeInput = true;
        currentPilot = pilot1;
        otherPilot = pilot2;
        rotationRadius = maxDistanceBetweenPilots;
        currentPilot.position = spawnPoint.position;
        OnSwitch.Invoke(currentPilot);
        rb = currentPilot.GetComponent<Rigidbody2D>();
        rb2 = otherPilot.GetComponent<Rigidbody2D>();
        MusicPlayer.Instance.PlayMenuMusic();
    }

    private void SwitchPilot()
    {
        Transform oldPilot = currentPilot;
        currentPilot = otherPilot;
        otherPilot = oldPilot;
        OnSwitch.Invoke(currentPilot);
    }

    private void Update()
    {
        HandleInput();

        if (rotationRadius < maxDistanceBetweenPilots && !pulling)
        {
            PushOtherPilot();
        }
        timer += Time.deltaTime * timescale;
        if(!sicko)
            timescale = maxDistanceBetweenPilots / (maxDistanceBetweenPilots * 0.2f + Mathf.Exp(rotationRadius) / Mathf.Exp(maxDistanceBetweenPilots) * 0.8f);

        lineRenderer.SetPosition(0, currentPilot.position);
        lineRenderer.SetPosition(1, otherPilot.position);
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void HandleInput()
    {
        if (takeInput)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                if (startGame)
                {
                    mainMenuUI.SetActive(false);
                    OnStart?.Invoke();
                    camAnim.SetTrigger("zoomOut");
                    startGame = false;
                    MusicPlayer.Instance.PlayGameplayMusic();
                }
                if (rotationRadius > 0.5f)
                {
                    PullOtherPilot();
                    msScale = 2;
                }
            }
            else if (!Input.GetKey(KeyCode.Space))
            {
                pulling = false;
                msScale = 1;
            }
        }
        
    }

    private void PullOtherPilot()
    {
        pulling = true;
        rotationRadius -= pullPilotSpeed * Time.deltaTime;
    }

    private void PushOtherPilot()
    {
        rotationRadius += pushPilotSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        UpdateRotation();
        rb.AddForce(movementInput * movementForce * forceScale);
        if (rb.velocity.magnitude >= maxMovementSpeed * msScale)
            rb.velocity = rb.velocity.normalized * maxMovementSpeed * msScale;
    }

    private void UpdateRotation()
    {
        rb2.position = rb.position + (Vector2)new Vector3(rotationRadius * Mathf.Cos(timer * Time.deltaTime * rotationSpeed), rotationRadius * Mathf.Sin(timer * Time.deltaTime * rotationSpeed)) ;
    }

    public void Respawn()
    {
        currentPilot.position = spawnPoint.position;
        sicko = false;
    }

    private void Screenshake()
    {
        camAnim.SetTrigger("shakeCam");
    }

    public void SickoMode()
    {
        if (sicko)
            return;
        IEnumerator SickoMode_Cor()
        {
            sicko = true;
            forceScale = 5;
            SFXPlayer.Instance.PlaySFX(sickomodeSFX);
            yield return new WaitForSeconds(3f);
            maxMovementSpeed = 30f;
            MusicPlayer.Instance.PlaySickoMode();
            PlaySawSFX(true);
            timescale = 5;
            pilot1.localScale = maxScale;
            pilot2.localScale = maxScale;
            yield return new WaitForSeconds(sickomodeTimer);
            pilot1.localScale = minScale;
            pilot2.localScale = minScale;
            sicko = false;
            forceScale = 1;
            MusicPlayer.Instance.PlayGameplayMusic();
            JuiceMeter.ResetJuice();
            PlaySawSFX(false);
            maxMovementSpeed = 15f;

        }
        StartCoroutine(SickoMode_Cor());
    }

    private void PlaySawSFX(bool v)
    {
        if (v)
            audioSource.Play();
        else
            audioSource.Stop();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pilot1.position, maxDistanceBetweenPilots);
    }

    private void OnDisable()
    {
        EnemyAIController.OnDeath -= Screenshake;
    }
}
