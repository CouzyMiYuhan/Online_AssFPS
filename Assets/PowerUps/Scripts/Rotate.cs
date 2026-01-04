using UnityEngine;

namespace PowerUps
{
    public class Rotate : MonoBehaviour
    {
        public float levitationHeight = 0.2f;
        public float levitationSpeed = 1.0f;

        private float startingLocalY;   // ➊ 声明变量

        void Start()
        {
            startingLocalY = transform.localPosition.y;   // ➋ 记录初始 local Y
        }

        void Update()
        {
            transform.Rotate(0, 0.4f, 0);

            float offset = Mathf.Sin(Time.time * levitationSpeed) * levitationHeight;

            // ➌ 按初始高度上下浮动，而不是强行改成 0.xx
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                startingLocalY + offset,
                transform.localPosition.z
            );
        }
    }
}
