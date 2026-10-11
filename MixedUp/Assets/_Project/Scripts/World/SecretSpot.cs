using System;
using UnityEngine;

namespace MixedUp
{
    /// <summary>
    /// A small secret to find on a map: look closer (E) and something funny happens (a message, a hop of the object). Finding
    /// secrets counts towards an achievement; the same one can be used again for fun after a short while.
    /// </summary>
    public class SecretSpot : MonoBehaviour, IInteractable
    {
        [Tooltip("Stable name, so finding the same secret twice does not count twice.")]
        public string id;
        public string promptKey = "prompt.secret";
        [Tooltip("Localization key of the message shown when it is used.")]
        public string toastKey;
        [Tooltip("What jumps and wobbles when it is used (optional).")]
        public Transform reaction;
        public float cooldownSeconds = 3f;

        float readyAt;
        float hop;
        Vector3 reactionBase;
        bool hasBase;

        public static event Action<SecretSpot> Found;

        public Transform InteractionTransform => transform;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Found = null;

        void Update()
        {
            if (reaction == null || hop <= 0f) return;
            if (!hasBase)
            {
                reactionBase = reaction.localPosition;
                hasBase = true;
            }
            hop = Mathf.MoveTowards(hop, 0f, Time.deltaTime * 2.2f);
            reaction.localPosition = reactionBase + Vector3.up * (Mathf.Sin(hop * Mathf.PI) * 0.35f);
            reaction.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(hop * Mathf.PI * 3f) * 8f);
        }

        public bool TryGetPrompt(PlayerInteractor who, out InteractionPrompt prompt)
        {
            prompt = InteractionPrompt.Allowed(promptKey);
            return true;
        }

        public void Interact(PlayerInteractor who)
        {
            if (Time.time < readyAt) return;
            readyAt = Time.time + cooldownSeconds;
            if (reaction != null && !hasBase)
            {
                reactionBase = reaction.localPosition;
                hasBase = true;
            }
            hop = 1f;
            if (!string.IsNullOrEmpty(toastKey)) GameEvents.RaiseToast(toastKey);
            AudioManager.Play(SfxId.Squeak, transform.position, 0.8f);
            Found?.Invoke(this);
        }
    }
}
