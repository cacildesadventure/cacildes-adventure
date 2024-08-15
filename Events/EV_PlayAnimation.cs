using System.Collections;
using UnityEngine;

namespace AF
{
    public class EV_PlayAnimation : EventBase
    {
        public Animator animator;

        public string animationName;

        public float crossFadeTime = 0.1f;

        [Header("Player Settings")]
        public bool usePlayerAnimator = false;

        PlayerManager playerManager;
        public Transform teleportPlayerToThisTransform;

        public override IEnumerator Dispatch()
        {
            yield return null;

            if (usePlayerAnimator)
            {
                if (playerManager == null)
                {
                    playerManager = FindAnyObjectByType<PlayerManager>(FindObjectsInactive.Include);
                }

                animator = playerManager.animator;

                if (teleportPlayerToThisTransform != null)
                {
                    playerManager.playerComponentManager.TeleportPlayer(teleportPlayerToThisTransform);
                }

            }

            if (crossFadeTime <= 0)
            {
                animator.Play(animationName);
            }
            else
            {
                animator.CrossFade(animationName, crossFadeTime);
            }

            yield return null;

        }
    }
}
