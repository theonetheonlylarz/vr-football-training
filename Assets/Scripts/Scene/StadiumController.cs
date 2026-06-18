using UnityEngine;
using System.Collections;
using FootballTraining.Core;

namespace FootballTraining.Scene
{
    /// <summary>
    /// Controls the stadium environment: lighting states, crowd reaction triggers,
    /// and field marker visibility.
    /// </summary>
    public class StadiumController : MonoBehaviour
    {
        [Header("Lighting")]
        [SerializeField] private Light _sunLight;
        [SerializeField] private Light[] _stadiumLights;
        [SerializeField] private float _nightLightIntensity = 3.0f;
        [SerializeField] private float _dayLightIntensity   = 1.2f;
        [SerializeField] private bool _isNightGame = false;

        [Header("Field")]
        [SerializeField] private GameObject _fieldRoot;
        [SerializeField] private GameObject _yardLineRoot;
        [SerializeField] private GameObject _hashMarkRoot;

        [Header("Line of Scrimmage Marker")]
        [SerializeField] private GameObject _losMarker;      // Blue LOS line
        [SerializeField] private GameObject _firstDownMarker;// Yellow 1st down line

        [Header("Crowd")]
        [SerializeField] private ParticleSystem _crowdCheers;
        [SerializeField] private AudioSource _ambientCrowd;

        private void Start()
        {
            ApplyLighting();
            GameManager.OnRepCompleted += OnRepCompleted;
            GameManager.OnSnapOccurred += OnSnap;
        }

        private void OnDestroy()
        {
            GameManager.OnRepCompleted -= OnRepCompleted;
            GameManager.OnSnapOccurred -= OnSnap;
        }

        private void ApplyLighting()
        {
            if (_isNightGame)
            {
                if (_sunLight != null) _sunLight.intensity = 0.1f;
                foreach (var l in _stadiumLights) l.intensity = _nightLightIntensity;
            }
            else
            {
                if (_sunLight != null) _sunLight.intensity = _dayLightIntensity;
                foreach (var l in _stadiumLights) l.intensity = 0.2f;
            }
        }

        private void OnSnap()
        {
            // Subtle crowd reaction to snap
            if (_ambientCrowd != null)
                StartCoroutine(BumpCrowdVolume(0.1f, 0.3f));
        }

        private void OnRepCompleted(bool correct, GapLocation gap, float rt)
        {
            if (correct && _crowdCheers != null)
                _crowdCheers.Play();
        }

        private IEnumerator BumpCrowdVolume(float bump, float duration)
        {
            float baseVol = _ambientCrowd.volume;
            _ambientCrowd.volume = baseVol + bump;
            yield return new WaitForSeconds(duration);
            _ambientCrowd.volume = baseVol;
        }

        public void SetLOSPosition(Vector3 worldPos)
        {
            if (_losMarker != null)
                _losMarker.transform.position = new Vector3(worldPos.x, 0.02f, worldPos.z);
        }

        public void SetFirstDownPosition(Vector3 worldPos)
        {
            if (_firstDownMarker != null)
                _firstDownMarker.transform.position = new Vector3(worldPos.x, 0.02f, worldPos.z);
        }
    }
}
