using System;
using UnityEngine;

namespace ithappy.Adventure_Land
{
    public enum AnimationType
    {
        None = 0,
        Idle = 1,
        Dance = 2,
        MilitaryStance = 4,
        SleepingOnDuty = 5,
        Meditation = 6,
    }

    public class CharacterAnimator : MonoBehaviour
    {
        [Serializable]
        public class JumpAnimationInfo
        {
            [SerializeField] AnimationClip _animationClip;
            [SerializeField] float _preparationPercent = 0.21f;
            [SerializeField] float _landPercent = 0.31f;

            public AnimationClip AnimationClip => _animationClip;
            public float PreparationTime { get; private set; }
            public float LandTime { get; private set; }
            public float JumpTime { get; private set; }

            public void Init()
            {
                if (AnimationClip == null)
                    return;
            
                PreparationTime = AnimationClip.length * _preparationPercent;
                LandTime = AnimationClip.length * _landPercent;
                JumpTime = AnimationClip.length * (1 - _preparationPercent - _landPercent);
            }
        }
        
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int HelloTrigger = Animator.StringToHash("Hello");
        private static readonly int ClimbUpTrigger = Animator.StringToHash("ClimbUp");
        private static readonly int ClimbDownTrigger = Animator.StringToHash("ClimbDown");
        private static readonly int HideTrigger = Animator.StringToHash("Hide");
        private static readonly int UnHideTrigger = Animator.StringToHash("UnHide");
        private static readonly int DanceTrigger = Animator.StringToHash("Dance");
        private static readonly int MilitaryStanceTrigger = Animator.StringToHash("MilitaryStance");
        private static readonly int SleepingOnDutyTrigger = Animator.StringToHash("SleepingOnDuty");
        private static readonly int Idle2Trigger = Animator.StringToHash("ChangeIdle");
        private static readonly int MeditationTrigger = Animator.StringToHash("Meditation");

        [SerializeField] private AnimationType _animationType = AnimationType.None;
        [SerializeField] private Animator _animator;
        [SerializeField] private JumpAnimationInfo _jumpAnimationinfo;
        [SerializeField] private AnimationClip _helloAnimation;

        private void Awake()
        {
            _jumpAnimationinfo.Init();
            switch (_animationType)
            {
                case AnimationType.None:
                    break;
                case AnimationType.Idle:
                    break;
                case AnimationType.Dance:
                    Dance(true);
                    break;
                case AnimationType.MilitaryStance:
                    MilitaryStance(true);
                    break;
                case AnimationType.SleepingOnDuty:
                    SleepingOnDuty(true);
                    break;
                case AnimationType.Meditation:
                    Meditation(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Initialize()
        {
        }

        public void Dispose()
        {
            
        }

        public void SetMoveSpeed(float speed)
        {
            _animator.SetFloat(Speed, speed);
        }
        
        public float GetMoveSpeed()
        {
            return _animator.GetFloat(Speed);
        }

        public JumpAnimationInfo Jump()
        {
            _animator.SetTrigger(JumpTrigger);
            return _jumpAnimationinfo;
        }

        public AnimationClip Hello()
        {
            _animator.SetTrigger(HelloTrigger);
            return _helloAnimation;
        }

        public AnimationClip Dance(bool isActive)
        {
            _animator.SetBool(DanceTrigger, isActive);
            return _helloAnimation;
        }

        public void MilitaryStance(bool isActive)
        {
            _animator.SetBool(MilitaryStanceTrigger, isActive);
        }
        
        public void Meditation(bool isActive)
        {
            _animator.SetBool(MeditationTrigger, isActive);
        }

        public void ChangeIdle()
        {
            _animator.SetTrigger(Idle2Trigger);
        }

        public void SleepingOnDuty(bool isActive)
        {
            _animator.SetBool(SleepingOnDutyTrigger, isActive);
        }

        public void ClimbUp(bool isActive)
        {
            _animator.SetBool(ClimbUpTrigger, isActive);
        }

        public void ClimbDown(bool isActive)
        {
            _animator.SetBool(ClimbDownTrigger, isActive);
        }

        public void Hide(bool isActive)
        {
            if (isActive)
            {
                _animator.SetTrigger(HideTrigger);
            }
            else
            {
                _animator.SetTrigger(UnHideTrigger);
            }
        }

        public void PauseAnimation()
        {
            _animator.speed = 0f;
        }

        public void ResumeAnimation()
        {
            _animator.speed = 1f;
        }
    }
}
