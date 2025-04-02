using UnityEngine;

public class Coral : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Transform wiggleDir;
    public Transform wiggleTarget;
    public float wiggleSpeed;
    public float wiggleMagnitude;

    private float  currentRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentRot = transform.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(0,0,currentRot+ Mathf.Sin(Time.time * wiggleSpeed) * wiggleMagnitude);
    }
}
