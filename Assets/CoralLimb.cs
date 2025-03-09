using UnityEngine;
using UnityEngine.Serialization;

public class CoralLimb : MonoBehaviour
{
    [Header("General")]
    public int length;
    [SerializeField] private Sprite sprite;
    
    [Header("Opening Gate")]
    [SerializeField] private float openingSpeed;
    [SerializeField] private float openingLineWidth;
    [SerializeField] private float openingDistBetweenPoints;

    [Header("Closing")] [SerializeField] private float closingSpeed;
    [SerializeField] private float closingLineWidth;
    [SerializeField] private float closingDistBetweenPoints;
    
    
    private Vector3[] _segmentPoses, _segmentV;
    public LineRenderer lineRenderer;

    public float currentSpeed, currentDistBetweenPoints;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        //lineRenderer.material.SetTexture("_MainTex",textureFromSprite(sprite));
        _segmentPoses = new Vector3[length];
        _segmentV = new Vector3[length];
        Close();
    }

    // Update is called once per frame
    // void Update()
    // {
    //     _segmentPoses[0] = transform.position;
    //     for(int i = 1; i < _segmentPoses.Length; i++)
    //     {
    //         _segmentPoses[i] = Vector3.SmoothDamp(_segmentPoses[i], _segmentPoses[i-1] + transform.right * currentDistBetweenPoints, ref _segmentV[i], 
    //             currentSpeed);
    //     }
    //
    //     lineRenderer.SetPositions(_segmentPoses);
    // }

    public void Open()
    {
        Debug.Log("Opening coral");
        currentSpeed = openingSpeed;
        currentDistBetweenPoints = openingDistBetweenPoints;

        lineRenderer.textureScale = new Vector2(1, openingLineWidth);

    }

    public void Close()
    {
        Debug.Log("Closing coral");
        currentSpeed = closingSpeed;
        currentDistBetweenPoints = closingDistBetweenPoints;
        lineRenderer.textureScale = new Vector2(1, closingLineWidth);
    }
    
    public static Texture2D textureFromSprite(Sprite sprite)
    {
        if(sprite.rect.width != sprite.texture.width){
            Texture2D newText = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height);
            Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x, 
                (int)sprite.textureRect.y, 
                (int)sprite.textureRect.width, 
                (int)sprite.textureRect.height );
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        } else
            return sprite.texture;
    }
    

    
}
