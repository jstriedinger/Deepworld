using UnityEngine;
using Gilzoide.UpdateManager;

public class SpriteWiggler : AManagedBehaviour, IUpdatable
{
    [SerializeField] private float amplitude = 30f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private bool counterClockwise = false;

    private float _initialRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _initialRot = transform.localEulerAngles.z;
    }

    public void ManagedUpdate()
    {
        float angle = (counterClockwise ? -1 : 1) * (Mathf.Sin(Time.time * frequency * Mathf.PI) * amplitude);
        transform.localRotation = Quaternion.Euler(0f,0f, angle + _initialRot);
    }
}
