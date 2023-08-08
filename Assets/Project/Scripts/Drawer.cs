using UnityEngine;
using UnityEngine.UI;

public class Drawer : MonoBehaviour
{
    [SerializeField] private ComputeShader _shader;
    [SerializeField] private RawImage _preview;
    [SerializeField] private RawImage _mnistPreview;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _checkButton;
    [SerializeField] private float _size = 5f;
    [SerializeField] private Mnist _mnist;

    private RenderTexture _renderTexture;
    private RenderTexture _mnistTexture;

    private int _kernelId;

    private void Start()
    {
        CreateRenderTexture();

        _preview.texture = _renderTexture;
        _mnistPreview.texture = _mnistTexture;

        _kernelId = _shader.FindKernel("CSMain");
        _shader.SetTexture(_kernelId, "_Texture", _renderTexture);

        _clearButton.onClick.AddListener(() =>
        {
            Clear(_renderTexture);
        });
        
        _checkButton.onClick.AddListener(Infer);
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

        Clear(_renderTexture);

        _mnistTexture = new RenderTexture(28, 28, 1);
        _mnistTexture.enableRandomWrite = true;
        _mnistTexture.Create();
    }

    private void Clear(RenderTexture texture)
    {
        RenderTexture backup = RenderTexture.active;
        RenderTexture.active = texture;
        GL.PushMatrix();
        GL.Clear(true, true, Color.black);
        GL.PopMatrix();
        RenderTexture.active = backup;
    }

    private void Infer()
    {
        Graphics.Blit(_renderTexture, _mnistTexture);
        _mnist.Execute(_mnistTexture);
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