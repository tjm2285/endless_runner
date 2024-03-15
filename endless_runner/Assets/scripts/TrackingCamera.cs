using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TrackingCamera : MonoBehaviour
{
    [SerializeField]
    AnimationCurve yCurve;

    Vector3 offset, position;

    float viewFactorX;
    ParticleSystem stars;
    void Awake()
    {        
        offset = transform.localPosition;
        Camera camera = GetComponent<Camera>();
        float viewFactorY = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        viewFactorX = viewFactorY  * camera.aspect;

        stars = GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = stars.shape;
        Vector3 position = shape.position;
        position.y = viewFactorY * position.z * 0.5f;
        shape.position = position;
        shape.scale = new Vector3(2f * viewFactorX, viewFactorY) * position.z;
    }

    public void StartNewGame()
    {
        Track(Vector3.zero);
        stars.Clear();
        stars.Emit(stars.main.maxParticles);
    }

    public void Track(Vector3 focusPoint)
    {
        position = focusPoint + offset;
        position.y = yCurve.Evaluate(position.y);
        transform.localPosition = position;
    }

    //returns a range made using the camera's X position and the view factor X scaled by the relative Z distance as its extents
    public FloatRange VisibleX(float z) =>
        FloatRange.PositionExtents(position.x, viewFactorX * (z - position.z));
}
