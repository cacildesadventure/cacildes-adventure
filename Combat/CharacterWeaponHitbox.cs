using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AF.Health;
using UnityEngine;
using UnityEngine.Events;

namespace AF
{
    public class CharacterWeaponHitbox : MonoBehaviour
    {
        [Header("Weapon")]
        public Weapon weapon;

        [Header("Trails")]
        public TrailRenderer trailRenderer;
        public BoxCollider hitCollider => GetComponent<BoxCollider>();

        [Header("Components")]
        public CharacterBaseManager character;

        [Header("Tags To Ignore")]
        public List<string> tagsToIgnore = new();

        [Header("SFX")]
        public AudioClip swingSfx;
        public AudioClip hitSfx;
        public AudioSource combatAudioSource;

        readonly List<DamageReceiver> damageReceiversHit = new();

        [Header("Events")]
        public UnityEvent onOpenHitbox;
        public UnityEvent onCloseHitbox;
        public UnityEvent onDamageInflicted;
        public UnityEvent onWeaponSpecial;
        public UnityEvent onEnvironmentCollision;

        [Header("Character Weapon Addons")]
        public CharacterTwoHandRef characterTwoHandRef;
        public CharacterWeaponBuffs characterWeaponBuffs;

        // Scene References
        Soundbank soundbank;
        WeaponCollisionFXManager weaponCollisionFXManager;

        // Internal flags
        bool canPlayHitSfx = true;

        List<BoxCollider> ownColliders = new();


        // Useful for throwable weapon situation
        [HideInInspector] public bool shouldDisableHitboxOnStart = true;

        private void Awake()
        {
            ownColliders = GetComponents<BoxCollider>()?.ToList();
        }

        void Start()
        {
            if (shouldDisableHitboxOnStart)
            {
                DisableHitbox();
            }
        }

        public void ShowWeapon()
        {
            gameObject.SetActive(true);

            if (characterTwoHandRef != null)
            {
                characterTwoHandRef.EvaluateTwoHandingUpdate();
            }
        }

        public void HideWeapon()
        {
            gameObject.SetActive(false);
        }

        public void EnableHitbox()
        {
            canPlayHitSfx = true;

            if (trailRenderer != null)
            {
                trailRenderer.Clear();

                trailRenderer.enabled = true;
            }

            if (hitCollider != null)
            {
                hitCollider.enabled = true;
            }

            if (swingSfx != null && HasSoundbank())
            {
                combatAudioSource.pitch = Random.Range(0.9f, 1.1f);
                combatAudioSource.Stop();

                soundbank.PlaySound(swingSfx, combatAudioSource);
            }

            onOpenHitbox?.Invoke();
        }

        public void DisableHitbox()
        {
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
            }

            if (hitCollider != null)
            {
                hitCollider.enabled = false;
            }

            if (ownColliders?.Count > 1)
            {
                foreach (var collider in ownColliders)
                {
                    collider.enabled = false;
                }
            }

            damageReceiversHit.Clear();
            onCloseHitbox?.Invoke();
        }

        bool IsCharacterConfused()
        {
            return character != null && character.isConfused;
        }


        public void OnTriggerEnter(Collider other)
        {
            if (HasWeaponCollisionManager())
            {
                weaponCollisionFXManager.EvaluateCollision(other, this.gameObject);
            }

            if (ShouldIgnoreCollision(other))
            {
                return;
            }

            if (other.TryGetComponent(out DamageReceiver damageReceiver) && IsValidDamageReceiver(damageReceiver))
            {
                if (IsCharacterConfused())
                {
                    damageReceiver = character.damageReceiver;
                }

                HandleDamage(damageReceiver);

                if (character is PlayerManager playerManager)
                {
                    HandlePlayerSpecificLogic(playerManager, damageReceiver);
                }

                damageReceiversHit.Add(damageReceiver);
            }
        }

        private bool ShouldIgnoreCollision(Collider other)
        {
            if (IsCharacterConfused())
            {
                return false;
            }

            if (tagsToIgnore.Contains(other.tag))
            {
                return true;
            }

            if (!character.HasAttackDamage())
            {
                return true;
            }

            return false;
        }

        private bool IsValidDamageReceiver(DamageReceiver damageReceiver)
        {
            // If character is confused, ignore the damage receiver as it'll be his own target
            if (IsCharacterConfused())
            {
                return true;
            }

            return damageReceiver != null && damageReceiver.character != character && !damageReceiversHit.Contains(damageReceiver);
        }

        private void HandleDamage(DamageReceiver damageReceiver)
        {
            damageReceiver.HandleIncomingDamage(character, (incomingDamage) =>
            {
                onDamageInflicted?.Invoke();

                if (hitSfx != null && canPlayHitSfx && character != null)
                {
                    canPlayHitSfx = false;
                    PlayHitSound();
                }
            }, IsCharacterConfused());
        }

        private void PlayHitSound()
        {
            if (HasSoundbank() && combatAudioSource != null)
            {
                combatAudioSource.pitch = Random.Range(0.9f, 1.1f);
                soundbank.PlaySound(hitSfx, combatAudioSource);
            }
        }

        private void HandlePlayerSpecificLogic(PlayerManager playerManager, DamageReceiver damageReceiver)
        {
            if (playerManager.playerBlockController.isCounterAttacking)
            {
                playerManager.playerBlockController.onCounterAttack?.Invoke();
            }

            damageReceiver?.health?.onDamageFromPlayer?.Invoke();

            if (weapon != null && damageReceiver?.health?.weaponRequiredToKill != null && damageReceiver.health.weaponRequiredToKill == weapon)
            {
                damageReceiver.health.hasBeenHitWithRequiredWeapon = true;
            }

            if (weapon != null)
            {
                playerManager.attackStatManager.attackSource = AttackStatManager.AttackSource.WEAPON;
            }
            else
            {
                playerManager.attackStatManager.attackSource = AttackStatManager.AttackSource.UNARMED;
            }
        }

        bool HasSoundbank()
        {
            if (soundbank == null)
            {
                soundbank = FindAnyObjectByType<Soundbank>(FindObjectsInactive.Include);

                return soundbank != null;
            }

            return true;
        }

        public bool UseCustomTwoHandTransform()
        {
            return characterTwoHandRef != null;
        }

        bool HasWeaponCollisionManager()
        {
            if (weaponCollisionFXManager == null)
            {
                weaponCollisionFXManager = FindAnyObjectByType<WeaponCollisionFXManager>(FindObjectsInactive.Include);

                return weaponCollisionFXManager != null;
            }

            return true;
        }
    }
}
