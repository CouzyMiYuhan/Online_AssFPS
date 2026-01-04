namespace ithappy.Adventure_Land
{
using UnityEngine;

public class EmissionOffsetAnimator : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0.1f, 0f); // скорость по X и Y
    private Material mat;
    private Vector2 currentOffset;

    void Start()
    {
        var renderer = GetComponent<Renderer>();
        if (!renderer)
        {
            Debug.LogError("Нет Renderer на объекте!");
            enabled = false;
            return;
        }

        mat = renderer.material;
        currentOffset = mat.mainTextureOffset;

        // Включаем эмиссию
        if (!mat.IsKeywordEnabled("_EMISSION"))
            mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        if (mat == null) return;

        currentOffset += scrollSpeed * Time.deltaTime;
        mat.mainTextureOffset = currentOffset;
    }
}

}