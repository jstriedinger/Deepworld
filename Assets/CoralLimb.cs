using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class CoralLimb : MonoBehaviour
{
    [Header("General")] 
    [SerializeField] private Texture2D coralTexture;
    public int length;
    public Transform wiggleDir;
    public Transform wiggleTarget;
    public float wiggleSpeed;
    [SerializeField] private bool throwBubbles;
    [SerializeField] private ParticleSystem vfxBubbles;
    [SerializeField] private float bubbleTime;
    private float _nextBubbles;
    private Tween _bubbleTween;
    
    [Header("Opening Gate")]
    [SerializeField] private float openingSpeed;
    [SerializeField] private float openingLineWidth;
    [SerializeField] private float openingDistBetweenPoints;
    [SerializeField] private float openingTweenDuration;

    [Header("Closing")] [SerializeField] private float closingSpeed;
    [SerializeField] private float closingLineWidth;
    public float closingDistBetweenPoints;
    
    
    public LineRenderer lineRenderer;
    public float currentSpeed, currentDistBetweenPoints;
    private bool isOpen = false;
    private Coroutine _bubbleCoroutine =  null;
    private MaterialPropertyBlock propertyBlock;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        //set our texture
        propertyBlock = new MaterialPropertyBlock();
        // Get the current property block
        lineRenderer.GetPropertyBlock(propertyBlock);
        // Set the texture only for this LineRenderer instance
        propertyBlock.SetTexture("_MainTex", coralTexture);
        // Apply the property block to the LineRenderer
        lineRenderer.SetPropertyBlock(propertyBlock);
        
        Close();
        _bubbleTween = transform.DOPunchPosition(new Vector3(0, .5f, 0), .5f, 1, 0f).OnComplete(
            () => { vfxBubbles.Stop(); vfxBubbles.Play(); }).SetAutoKill(false).Pause();
        
        if(!throwBubbles)
            Destroy(vfxBubbles.gameObject);
    }

    IEnumerator ThrowBubbles()
    {
        while (throwBubbles && !isOpen)
        {
            yield return new WaitForSeconds(bubbleTime);
            vfxBubbles.transform.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            _bubbleTween.Restart();

        }

    }

    

    public void Open()
    {
        Debug.Log("Opening coral");
        currentSpeed = openingSpeed;
        isOpen = true;
        //lineRenderer.textureScale = new Vector2(1, openingLineWidth);
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => currentDistBetweenPoints, x => currentDistBetweenPoints = x,
            closingDistBetweenPoints +0.2f, openingTweenDuration * 0.6f));
        seq.Append(DOTween.To(() => currentDistBetweenPoints, x => currentDistBetweenPoints = x,
            openingDistBetweenPoints, openingTweenDuration));
        seq.Join(DOTween.To(() => lineRenderer.textureScale.y, x =>
            {
                Vector2 tmp = lineRenderer.textureScale;
                tmp.y = x;
                lineRenderer.textureScale = tmp;
            },
            openingLineWidth, openingTweenDuration+.1f));
            
        //DOTween.To(() => currentDistBetweenPoints, x => currentDistBetweenPoints = x,
          //  openingDistBetweenPoints, 2f).SetEase(Ease.InBounce);
        //currentDistBetweenPoints = openingDistBetweenPoints;

        if(_bubbleCoroutine != null)
            StopCoroutine(_bubbleCoroutine);

    }

    public void Close()
    {
        Debug.Log("Closing coral");
        isOpen = false;
        currentSpeed = closingSpeed;
        lineRenderer.textureScale = new Vector2(1, closingLineWidth);
        currentDistBetweenPoints = closingDistBetweenPoints;
        if(throwBubbles)
            _bubbleCoroutine = StartCoroutine(ThrowBubbles());
    }
    
   
    

    
}
