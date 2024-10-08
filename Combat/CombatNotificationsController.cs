using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Localization.Settings;

namespace AF
{
    public class CombatNotificationsController : MonoBehaviour
    {
        public Transform notificationRootTransform;
        public List<CombatNotificationEntry> combatNotificationEntries = new();
        CombatNotificationManager combatNotificationManager;

        public float yOffset = .25f;

        private void Awake()
        {
            combatNotificationManager = FindObjectOfType<CombatNotificationManager>(true);
        }

        public void AddNotification(string text, Color color)
        {
            // First, check if combatNotificationEntries contains inactive game objects, which essentialy means we need to clean up the list
            combatNotificationEntries = combatNotificationEntries.Where(x => x != null && x.transform != null && x.isActiveAndEnabled).ToList();

            // Update vertical position of existing elements 
            combatNotificationEntries.ForEach(element =>
            {
                element.transform.position = new Vector3(element.transform.position.x, element.transform.position.y + yOffset, element.transform.position.z);
            });

            var instance = combatNotificationManager.GetInstance();
            if (instance == null)
            {
                return;
            }

            instance.transform.position = notificationRootTransform.transform.position;
            instance.transform.SetParent(notificationRootTransform);
            instance.currentDuration = 0f;
            instance.textMeshPro.color = color;
            instance.textMeshPro.text = text;

            combatNotificationEntries.Add(instance);
        }

        public void ShowDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.damage);
        }
        public void ShowFireDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.fireDamage);
        }
        public void ShowFrostDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.frostDamage);
        }
        public void ShowLightningDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.lightningDamage);
        }
        public void ShowDarknessDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.darknessDamage);
        }
        public void ShowWaterDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.waterDamage);
        }
        public void ShowMagicDamage(int amount)
        {
            AddNotification("- " + amount, combatNotificationManager.magicDamage);
        }
        public void ShowStatusFullAmountEffect(string displayedStatusEffectName, Color statusEffectColor)
        {
            AddNotification(displayedStatusEffectName.ToLower(), statusEffectColor);
        }
        public void ShowStatusEffectAmount(string statusEffect, float amount, Color statusEffectColor)
        {
            if (amount <= 0)
            {
                return;
            }

            AddNotification("- " + amount + " " + LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "from") + " " + statusEffect.ToLower(), statusEffectColor);
        }
        public void ShowCritical(float amount)
        {
            AddNotification("- " + amount + " " + LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "from critical attack"), combatNotificationManager.criticalDamage);
        }
        public void ShowBackstab(float amount)
        {
            AddNotification("- " + amount + " " + LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "from backstab attack"), combatNotificationManager.criticalDamage);
        }
        public void ShowGuardCounter(float amount)
        {
            AddNotification("- " + amount + " " + LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "from guard counter attack"), combatNotificationManager.criticalDamage);
        }
        public void ShowRageCounter(float amount)
        {
            AddNotification("- " + amount + " " + LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "from rage buildup attack"), combatNotificationManager.criticalDamage);
        }
        public void ShowPostureBroken(float amount)
        {
            AddNotification("- " + amount + " " + LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "from broken posture"), combatNotificationManager.criticalDamage);
        }
    }

}
