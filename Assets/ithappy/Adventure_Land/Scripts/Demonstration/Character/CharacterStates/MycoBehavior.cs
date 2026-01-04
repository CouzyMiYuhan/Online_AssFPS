using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class MycoBehavior : CharacterStateBase
    {
        private Vector2 _moveCooldownMinMax = new Vector2(3f, 7f);
        private float _currentMoveCooldown;
        private List<Transform> _targetPoints;
        private int _currentPointIndex = 0;
        private bool _isMoving;
        private MovementBase _movement;
        
        public MycoBehavior(CharacterBase context, MovementBase movement, List<Transform> targetPoints) : base(context)
        {
            _targetPoints =  targetPoints;
            _movement =  movement;
            
            _currentMoveCooldown = Random.Range(_moveCooldownMinMax.x, _moveCooldownMinMax.y);
        }

        public override void Update()
        {
            if (_isMoving)
            {
                return;
            }
            
            _currentMoveCooldown -= Time.deltaTime;
            if (_currentMoveCooldown <= 0)
            {
                _isMoving = true;
                _currentMoveCooldown = Random.Range(_moveCooldownMinMax.x, _moveCooldownMinMax.y);
                CharacterBase.StartCoroutine(Move());
            }
        }

        private IEnumerator Move()
        {
            // Запоминаем текущий поворот
            Quaternion startRotation = _movement.MoveParent.rotation;
            // Берем поворот целевой точки
            Quaternion targetRotation = _targetPoints[_currentPointIndex].rotation;
    
            // Плавный поворот к ориентации целевой точки
            float elapsed = 0f;
            float rotateTime = 0.5f;
    
            while (elapsed < rotateTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / rotateTime);
                _movement.MoveParent.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
    
            // Точно устанавливаем финальный поворот
            _movement.MoveParent.rotation = targetRotation;
    
            // Начинаем движение к точке
            _movement.MoveToTarget(_targetPoints[_currentPointIndex].position, (isReached) =>
            {
                _isMoving = false;
                _currentPointIndex++;
                if (_currentPointIndex >= _targetPoints.Count)
                {
                    _currentPointIndex = 0;
                }
            });
        }

        public override void Exit()
        {
            
        }

        public override bool ShouldEnter()
        {
            return true;
        }
    }
}
