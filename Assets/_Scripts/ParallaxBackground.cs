using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ParallaxBackground : MonoBehaviour
{
    private Transform camTransform;
    private Vector3 lastCamPos;
    private float textureUnitSizeX;
    [SerializeField] float parallaxMultiplier;
    [SerializeField] CinemachineVirtualCamera vCam;
    // Start is called before the first frame update
    void Start()
    {
        camTransform = vCam.transform;
        lastCamPos = camTransform.position;
        Sprite sp = GetComponent<SpriteRenderer>().sprite;
        Texture2D text = sp.texture;
        textureUnitSizeX = text.width / sp.pixelsPerUnit;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 deltaMov = new Vector3( camTransform.position.x - lastCamPos.x, 0, 0);
        transform.position += deltaMov * parallaxMultiplier;
        lastCamPos = camTransform.position;

        if (Mathf.Abs(camTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPosX = (camTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(camTransform.position.x + offsetPosX, transform.position.y);
        }


    }
}
