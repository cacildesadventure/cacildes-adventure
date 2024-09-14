using System.Collections;
using AF.Combat;
using AF.Ladders;
using UnityEngine;

namespace AF
{
    public class PlayerCombatController : MonoBehaviour
    {

        public readonly int hashLightAttack1 = Animator.StringToHash("Light Attack 1");
        public readonly string hashLightAttack2 = "Light Attack 2";
        public readonly int hashHeavyAttack1 = Animator.StringToHash("Heavy Attack 1");

        public readonly string hashUpperLightAttack1 = "Upper Light Attack 1";
        public readonly string hashUpperLightAttack2 = "Upper Light Attack 2";
        public readonly string hashUpperHeavyAttack1 = "Upper Heavy Attack 1";
        public readonly int hashSpecialAttack = Animator.StringToHash("Special Attack");
        public readonly int hashJumpAttack = Animator.StringToHash("Jump Attack");

        [Header("Attack Combo Index")]
        public float maxIdleCombo = 2f;
        [SerializeField] int lightAttackComboIndex = 0;

        [Header("Flags")]
        public bool isCombatting = false;
        public bool isLightAttacking = false;

        [Header("Components")]
        public PlayerManager playerManager;
        public Animator animator;
        public UIManager uIManager;

        [Header("Heavy Attacks")]
        public int unarmedHeavyAttackBonus = 35;

        [Header("UI")]
        public MenuManager menuManager;

        [Header("Databases")]
        public EquipmentDatabase equipmentDatabase;

        [Header("Flags")]
        public bool isHeavyAttacking = false;
        public bool isJumpAttacking = false;

        // Coroutines
        Coroutine ResetLightAttackComboIndexCoroutine;

        public readonly string SpeedMultiplierHash = "SpeedMultiplier";

        private void Start()
        {
            animator.SetFloat(SpeedMultiplierHash, 1f);
        }

        public void ResetStates()
        {
            isJumpAttacking = false;
            isHeavyAttacking = false;
            isLightAttacking = false;
            animator.SetFloat(SpeedMultiplierHash, 1f);
        }

        public void OnLightAttack()
        {
            if (CanLightAttack())
            {
                HandleLightAttack();
            }
        }

        public void OnHeavyAttack()
        {
            if (CanHeavyAttack())
            {
                HandleHeavyAttack(false);
            }
        }

        public bool IsAttacking()
        {
            return isLightAttacking || isHeavyAttacking || isJumpAttacking;
        }

        public void HandleLightAttack()
        {
            isHeavyAttacking = false;
            isLightAttacking = true;

            if (playerManager.thirdPersonController.Grounded)
            {
                if (playerManager.playerBackstabController.PerformBackstab())
                {
                    return;
                }

                if (lightAttackComboIndex > 1)
                {
                    lightAttackComboIndex = 0;
                }

                if (lightAttackComboIndex == 0)
                {
                    if (equipmentDatabase.IsUsingUpperLayerAnimations())
                    {
                        playerManager.animator.CrossFade(hashUpperLightAttack1, 0.2f);
                    }
                    else
                    {
                        playerManager.PlayBusyHashedAnimationWithRootMotion(hashLightAttack1);
                    }
                }
                else if (lightAttackComboIndex == 1)
                {
                    if (equipmentDatabase.IsUsingUpperLayerAnimations())
                    {
                        playerManager.animator.CrossFade(hashUpperLightAttack2, 0.2f);
                    }
                    else
                    {
                        playerManager.PlayCrossFadeBusyAnimationWithRootMotion(hashLightAttack2, 0.2f);
                    }
                }

                HandleAttackSpeed();
            }
            else
            {
                HandleJumpAttack();
            }

            lightAttackComboIndex++;
            playerManager.staminaStatManager.DecreaseLightAttackStamina();

            if (ResetLightAttackComboIndexCoroutine != null)
            {
                StopCoroutine(ResetLightAttackComboIndexCoroutine);
            }
            ResetLightAttackComboIndexCoroutine = StartCoroutine(_ResetLightAttackComboIndex());
        }

        IEnumerator _ResetLightAttackComboIndex()
        {
            yield return new WaitForSeconds(maxIdleCombo);
            lightAttackComboIndex = 0;
        }

        void HandleAttackSpeed()
        {
            Weapon currentWeapon = equipmentDatabase.GetCurrentWeapon();
            if (equipmentDatabase.isTwoHanding == false && currentWeapon != null && currentWeapon.oneHandAttackSpeedPenalty != 1)
            {
                animator.SetFloat(SpeedMultiplierHash, currentWeapon.oneHandAttackSpeedPenalty);
            }
            else if (equipmentDatabase.isTwoHanding && currentWeapon != null && currentWeapon.twoHandAttackSpeedPenalty != 1)
            {
                animator.SetFloat(SpeedMultiplierHash, currentWeapon.twoHandAttackSpeedPenalty);
            }
            else
            {
                animator.SetFloat(SpeedMultiplierHash, 1f);
            }
        }

        void HandleJumpAttack()
        {
            isHeavyAttacking = false;
            isLightAttacking = false;
            isJumpAttacking = true;

            playerManager.playerWeaponsManager.HideShield();

            playerManager.playerAnimationEventListener.OpenRightWeaponHitbox();

            playerManager.PlayBusyHashedAnimationWithRootMotion(hashJumpAttack);
            playerManager.playerComponentManager.DisableCollisionWithEnemies();
        }

        public void HandleHeavyAttack(bool isCardAttack)
        {
            if (isCombatting || playerManager.thirdPersonController.Grounded == false)
            {
                return;
            }

            isLightAttacking = false;
            isHeavyAttacking = true;

            playerManager.playerWeaponsManager.HideShield();

            if (isCardAttack)
            {
                playerManager.PlayBusyHashedAnimationWithRootMotion(hashSpecialAttack);
            }
            else
            {
                if (equipmentDatabase.IsUsingUpperLayerAnimations())
                {
                    playerManager.animator.CrossFade(hashUpperHeavyAttack1, 0.2f);
                }
                else
                {
                    playerManager.PlayBusyHashedAnimationWithRootMotion(hashHeavyAttack1);
                }
            }

            playerManager.staminaStatManager.DecreaseHeavyAttackStamina();

            HandleAttackSpeed();
        }

        public bool CanLightAttack()
        {
            if (!this.isActiveAndEnabled)
            {
                return false;
            }

            if (CanAttack() == false)
            {
                return false;
            }

            if (equipmentDatabase.IsStaffEquipped() || equipmentDatabase.IsBowEquipped())
            {
                return false;
            }

            return playerManager.staminaStatManager.HasEnoughStaminaForLightAttack();
        }

        public bool CanHeavyAttack()
        {
            if (CanAttack() == false)
            {
                return false;
            }

            return playerManager.staminaStatManager.HasEnoughStaminaForHeavyAttack();
        }

        bool CanAttack()
        {
            if (playerManager.IsBusy())
            {
                return false;
            }

            if (playerManager.characterBlockController.isBlocking)
            {
                return false;
            }

            if (menuManager.isMenuOpen)
            {
                return false;
            }

            if (playerManager.playerShootingManager.isAiming)
            {
                return false;
            }

            if (playerManager.climbController.climbState != ClimbState.NONE)
            {
                return false;
            }

            if (playerManager.dodgeController.isDodging)
            {
                return false;
            }

            if (uIManager.IsShowingGUI())
            {
                return false;
            }

            if (playerManager.thirdPersonController.isSwimming)
            {
                return false;
            }

            return true;
        }

        private void OnDisable()
        {
            ResetStates();
        }


        public void HandlePlayerAttack(IDamageable damageable, Weapon weapon)
        {
            if (damageable is not DamageReceiver damageReceiver)
            {
                return;
            }

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
    }
}
