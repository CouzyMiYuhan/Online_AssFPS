using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class CharacterSoundController : MonoBehaviour
    {
        [SerializeField] private AudioSource _helloAudioSource;
        [SerializeField] private AudioSource _leftStepAudioSource;
        [SerializeField] private AudioSource _rightStepAudioSource;
        [SerializeField] private AudioSource _jumpAudioSource;

        private CharacterAnimatorInvoker _animatorInvoker;

        private void Start()
        {
            _animatorInvoker = GetComponentInChildren<CharacterAnimatorInvoker>();

            if (_animatorInvoker != null)
            {
                _animatorInvoker.OnCharacterHello += Hello;
                _animatorInvoker.OnCharacterLeftStep += LeftStep;
                _animatorInvoker.OnCharacterRightStep += RightStep;
                _animatorInvoker.OnCharacterJump += Jump;
            }
        }

        private void Hello()
        {
            _helloAudioSource?.Play();
        }

        private void LeftStep()
        {
            _leftStepAudioSource?.Play();
        }

        private void RightStep()
        {
            _rightStepAudioSource?.Play();
        }

        private void Jump()
        {
            _jumpAudioSource?.Play();
        }
    }
}
