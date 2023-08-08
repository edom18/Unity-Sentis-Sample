using UnityEngine;
using UnityEngine.UI;

public class Drawer : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private RawImage _preview;
    [SerializeField] private float _size = 5f;

    private RenderTexture _renderTexture;

    private int _kernelId;

    private void Start()
    {
        CreateRenderTexture();

        _preview.texture = _renderTexture;

        _kernelId = _shader.FindKernel("CSMain");
        _shader.SetTexture(_kernelId, "_Texture", _renderTexture);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            TryDraw();
        }
    }

    private void CreateRenderTexture()
    {
        _renderTexture = new RenderTexture(256, 256, 1);
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();

        RenderTexture backup = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        GL.PushMatrix();
        GL.Clear(true, true, Color.white);
        GL.PopMatrix();
        RenderTexture.active = backup;
    }

    private void TryDraw()
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_preview.rectTransform, Input.mousePosition, null, out var localPoint))
        {
            return;
        }

        Draw(localPoint);
    }

    private void Draw(Vector2 points)
    {
        _shader.SetVector("_Points", new Vector4(points.x, points.y, 0, 0));
        _shader.SetFloat("_Size", _size);
        _shader.Dispatch(_kernelId, _renderTexture.width / 8, _renderTexture.height / 8, 1);
    }
}