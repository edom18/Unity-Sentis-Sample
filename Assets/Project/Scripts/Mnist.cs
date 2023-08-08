using Unity.Sentis;
using UnityEngine;
using TMPro;

public class Mnist : MonoBehaviour
{
    [SerializeField] private ModelAsset _modelAsset;
    [SerializeField] private TMP_Text _result;

    private static readonly int s_width = 28;
    private static readonly int s_height = 28;

    private Model _runtimeModel;
    private IWorker _engine;

    private void Start()
    {
        _runtimeModel = ModelLoader.Load(_modelAsset);
        _engine = WorkerFactory.CreateWorker(BackendType.GPUCompute, _runtimeModel);
    }

    private TensorFloat GetTensor(Texture texture)
    {
        TensorFloat tensor = TextureConverter.ToTensor(texture);
        TensorShape shape = new TensorShape(1, 784);
        return tensor.ShallowReshape(shape) as TensorFloat;
    }

    public void Execute(Texture texture)
    {
        TensorFloat inputTensor = GetTensor(texture);
        _engine.Execute(inputTensor);

        TensorFloat outputTensor = _engine.PeekOutput() as TensorFloat;

        float[] results = outputTensor.ToReadOnlyArray();
        int number = Inference(results);

        _result.text = number.ToString();
        
        Debug.Log($"Result: {number.ToString()}");
    }

    private int Inference(float[] results)
    {
        int number = -1;
        float max = float.MinValue;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] > max)
            {
                max = results[i];
                number = i;
            }
        }

        return number;
    }

    private void OnDestroy()
    {
        _engine.Dispose();
    }
}