﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class HealthComponent : MonoBehaviour, HasHealth {

    public bool isDead = false;
    public bool canBeDamaged = true;
    public int currentHealth;
    public int currentArmor;
    public int startHealth = 100;
    public int startArmor = 0;
    public int maxHealth = 100;
    public int maxArmor = 100;
    public bool damaged = false;
    public AudioClip gainHealthClip;
    public AudioClip gainArmorClip;
    public AudioClip hurtClip;
    public AudioClip armorHurtClip;
    public AudioClip deathClip;

    private AudioSource audioSource;

    public bool CanBeDamaged => canBeDamaged;
    SoundList database;

    void Awake ()
    {
        currentHealth = startHealth;
        currentArmor = startArmor;
        database = GameManager.Instance.audioDatabase;
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
    }

    public void ApplyDamage(int amount)
    {
        damaged = true;
        currentArmor -= amount;
        if (currentArmor<0)
        {
            currentHealth += currentArmor;
            currentArmor = 0;
            PlayOneShot(hurtClip);
        }
        else
        {
            PlayOneShot(armorHurtClip);
        }

        if (currentHealth <= 0 && !isDead)
        {
            Death();
        }

        if (currentHealth < 0)
            currentHealth = 0;
    }

    public int Health => currentHealth;

    void Death()
    {
        isDead = true;
        audioSource.PlayOneShot(deathClip, 0.5f);
        // Broadcast 
        List<DeathHandler> handlers;
        gameObject.GetInterfaces<DeathHandler>(out handlers);
        bool destroyOnDeath = true;

        if(handlers != null && handlers.Count > 0){
            foreach(DeathHandler handler in handlers) {
                destroyOnDeath &= handler.HandleDeath();
            }
        }

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    public void IncreaseHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        PlayOneShot(gainHealthClip);
    }

    public void IncreaseArmor(int amount)
    {
        currentArmor = Mathf.Clamp(currentArmor + amount, 0, maxArmor);
        PlayOneShot(gainArmorClip);
    }

    private void PlayOneShot(AudioClip clip, bool force = false){
        if(force || !audioSource.isPlaying){
            audioSource.PlayOneShot(clip, 0.5f);
        }
    }
}
