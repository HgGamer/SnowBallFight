using System;
using System.Collections;
using System.Collections.Generic;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
public class PuppetController : EntityController
{

    public AudioSource hitSound;
    public AudioSource throwSound;
    public AudioSource fallSound;
    public AudioSource walkSound;

    // Sound settings
    [Header("Sound Settings")]
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;
    [SerializeField] private float pitchChangeInterval = 0.3f;
    private float nextPitchChangeTime;

    public GameObject snowballInHand;
    private Animator animator;
    private PlayerController Owner;
    public Puppet Puppet;
    public GameObject yeti;
    public GameObject hole;
    private float speed = 0;
    void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Set initial random pitch
        if (walkSound != null)
        {
            walkSound.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        }
    }
    public void Spawn(Puppet puppet, PlayerController owner)
    {
        Debug.Log($"PuppetController: Spawn: {puppet.EntityId}");
        base.Spawn(puppet.EntityId);
        this.Owner = owner;
        this.Puppet = puppet;
        //GetComponentInChildren<TMPro.TextMeshProUGUI>().text = owner.Username;
    }

    public virtual void OnEntityUpdated(Puppet newVal)
    {
        if (snowballInHand != null && snowballInHand.activeSelf && !newVal.HasSnowball)
        {
            throwSound.Play();
        }
        snowballInHand.SetActive(newVal.HasSnowball);

        Debug.Log($"PuppetController: OnEntityUpdated: {newVal.EntityId} , {newVal.PlayerId},  has snowball:  {newVal.HasSnowball}");
        this.Puppet = newVal;
        var states = newVal.CurrentStates;
        animator.SetFloat("Speed", newVal.Speed);
        speed = newVal.Speed;
       
      
        
        foreach (var state in states)
        {
            OnStateChanged(state);
        }
    }
    IEnumerator ShowYeti()
    {
        yield return new WaitForSeconds(1f);
        Instantiate(hole, transform.position, Quaternion.identity);
        Instantiate(yeti, transform.position, Quaternion.identity);

    }
    public virtual void OnStateChanged(PlayerActions state)
    {
        switch (state)
        {
            case PlayerActions.Throw:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Throw");
                break;
            case PlayerActions.Hit:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Hit");
                if (hitSound != null)
                {
                    // Add pitch variation to hit sound too
                    hitSound.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                    hitSound.Play();
                }
                fallSound.PlayDelayed(0.25f);
                ShowYeti();
                break;
            case PlayerActions.Craft:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Craft");
                break;
            case PlayerActions.Standup:
                Debug.Log($"PuppetController: OnStateChanged: {EntityId} , {state}");
                animator.SetTrigger("Standup");
                break;
        }
    }
    public override void OnDelete(EventContext context)
    {
        Debug.Log($"PuppetController: OnDelete: {EntityId}");
        base.OnDelete(context);
        Owner.OnPuppetDeleted(this);
    }
    void Update()
    {
        base.Update();
        
        // Update pitch periodically while walking
        if (walkSound != null && walkSound.isPlaying && Time.time > nextPitchChangeTime)
        {
            walkSound.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
            nextPitchChangeTime = Time.time + pitchChangeInterval;
        }
        HandleWalkSound(speed);
    }
    
    private void HandleWalkSound(float speed)
    {
        if (walkSound == null) return;
        
        if (speed > 0.01f)
        {
            if (!walkSound.isPlaying)
            {
                // Set a random pitch before playing
                walkSound.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
                walkSound.Play();
                nextPitchChangeTime = Time.time + pitchChangeInterval;
            }
        }
        else
        {
            walkSound.Stop();
        }
    }
}
