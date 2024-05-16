using UnityEngine;
 
public class Segment
{
    public Vector3 startingPosition;
    public Vector3 endingPosition;
    public float length;
 
    public void Follow(Segment targetSegment, float smoothFactor)
    {
        Follow(targetSegment.startingPosition, smoothFactor);
    }
    
    public void Follow(Vector3 targetPosition, float smoothFactor)
    {
        Vector3 referenceVelocity = Vector3.zero;
        endingPosition = targetPosition;
        Vector3 difference = endingPosition-startingPosition;
        Vector3 normalized = difference.normalized;
        startingPosition = Vector3.SmoothDamp(startingPosition, endingPosition-(normalized*length), ref referenceVelocity, smoothFactor);
    }
 
    public void AnchorStartAt(Vector3 targetPosition)
    {
        Vector3 difference = endingPosition-startingPosition;
        startingPosition = targetPosition;
        endingPosition = startingPosition+difference;
    }
 
    #region videospecific
    public void RotateTowards(Vector3 targetPosition)
    {
        Vector3 difference = targetPosition-startingPosition;
        endingPosition = startingPosition+(difference.normalized*length);
    }
    public void AnchorEndAt(Vector3 targetPosition)
    {
        Vector3 difference = endingPosition-startingPosition;
        endingPosition = targetPosition;
        startingPosition = endingPosition-difference;
    }
    #endregion
}
