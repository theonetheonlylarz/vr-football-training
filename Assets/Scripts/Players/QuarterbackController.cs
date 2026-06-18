using UnityEngine;
using System.Collections;
using FootballTraining.Core;

namespace FootballTraining.Players
{
    /// <summary>
    /// Controls the quarterback — under-center snap, shotgun snap, drop back, and throw.
    /// </summary>
    public class QuarterbackController : OffensivePlayerController
    {
        [Header("QB Settings")]
        [SerializeField] private float _dropDepthYards = 5f;
        [SerializeField] private bool _isUnderCenter = true;

        private static readonly int HashSnap      = Animator.StringToHash("Snap");
        private static readonly int HashDropBack  = Animator.StringToHash("DropBack");
        private static readonly int HashThrow     = Animator.StringToHash("Throw");
        private static readonly int HashHandoff   = Animator.StringToHash("Handoff");
        private static readonly int HashShotgunSnap = Animator.StringToHash("ShotgunSnap");

        public void SetShotgun(bool isShotgun) => _isUnderCenter = !isShotgun;

        public void ExecuteSnap(bool isRunPlay)
        {
            StartCoroutine(SnapSequence(isRunPlay));
        }

        private IEnumerator SnapSequence(bool isRunPlay)
        {
            // Snap animation
            TriggerAnimation(_isUnderCenter ? "Snap" : "ShotgunSnap");
            yield return new WaitForSeconds(0.15f);

            if (isRunPlay)
            {
                TriggerAnimation("Handoff");
                yield return new WaitForSeconds(0.3f);
                yield return DropBack(bootleg: false, dropDepth: 1.5f);
            }
            else
            {
                yield return DropBack(bootleg: false, dropDepth: _dropDepthYards);
                yield return new WaitForSeconds(0.8f);
                TriggerAnimation("Throw");
            }
        }

        private IEnumerator DropBack(bool bootleg, float dropDepth)
        {
            TriggerAnimation("DropBack");
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos - transform.forward * dropDepth;
            float duration = dropDepth / 7f;  // ~7 yards/sec
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }
        }
    }
}
