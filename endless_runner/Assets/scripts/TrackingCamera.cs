using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackingCamera : MonoBehaviour
{
    Vector3 offset, position;

    float viewFactorX;
    void Awake()
    {        
        offset = transform.localPosition;
        Camera camera = GetComponent<Camera>();
        float viewFactorY = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        viewFactorX = viewFactorY  * camera.aspect;
    }

    public void StartNewGame()
    {
        Track(Vector3.zero);   
    }

    public void Track(Vector3 focusPoint)
    {
        position = focusPoint + offset;
        transform.localPosition = position;
    }

    //returns a range made using the camera's X position and the view factor X scaled by the relative Z distance as its extents
    public FloatRange VisibleX(float z) =>
        FloatRange.PositionExtents(position.x, viewFactorX * (z - position.z));
}
