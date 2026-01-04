using System.Collections;
using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class BonusItem : MonoBehaviour
    {
        [SerializeField] private float _reload;

        private MeshRenderer _meshRenderer;
        private Collider _collider;
        private AudioSource _audioSource;
        private bool _isReserved;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            BonusSeeker bonusSeeker = collider.GetComponent<BonusSeeker>();
            PlayerCharacterInput player = collider.GetComponent<PlayerCharacterInput>();
            if (bonusSeeker != null || player != null)
            {
                _meshRenderer.enabled = false;
                _collider.enabled = false;
                if (_audioSource != null)
                {
                    _audioSource.Play();
                }
                StartCoroutine(ReloadProgress());
            }
        }

        public bool TryReserved()
        {
            if (!_isReserved)
            {
                _isReserved = true;
                return true;
            }
            return false;
        }

        private IEnumerator ReloadProgress()
        {
            yield return new WaitForSeconds(_reload);

            _collider.enabled = true;
            _meshRenderer.enabled = true;
            _isReserved = false;
        }
    }
}
