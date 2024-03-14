using UnityEngine;

public class SkylineGenerator : MonoBehaviour
{
    const float border = 10f;

    [SerializeField]
    SkylineObject[] prefabs;

    [SerializeField]
    float distance;

    [SerializeField]
    FloatRange altitude;

    [SerializeField]
    FloatRange gapLength, sequenceLength;

    Vector3 endPosition;

    float sequenceEndX;

    SkylineObject leftmost, rightmost;

    SkylineObject GetInstance()
    {
        SkylineObject instance = prefabs[Random.Range(0, prefabs.Length)].GetInstance();
        instance.transform.SetParent(transform, false);
        return instance;
    }

    public void FillView(TrackingCamera view)
    {
        FloatRange visibleX = view.VisibleX(distance).GrowExtents(border);
        while (leftmost != rightmost && leftmost.MaxX < visibleX.min)
        {
            leftmost = leftmost.Recycle();
        }
        while (endPosition.x < visibleX.max)
        {
            if (endPosition.x > sequenceEndX)
            {
                StartNewSequence(gapLength.RandomValue, sequenceLength.RandomValue);
            }
            rightmost = rightmost.Next = GetInstance();
            endPosition = rightmost.PlaceAfter(endPosition);
        }
    }

    public void StartNewGame(TrackingCamera view)
    {
        while (leftmost != null)
        {
            leftmost = leftmost.Recycle();
        }
        FloatRange visibleX = view.VisibleX(distance).GrowExtents(border);
        endPosition = new Vector3(visibleX.min, altitude.RandomValue, distance);
        sequenceEndX = sequenceLength.RandomValue;

        leftmost = rightmost = GetInstance();
        endPosition = rightmost.PlaceAfter(endPosition);
        FillView(view);
    }

    void StartNewSequence(float gap, float sequence)
    {
        endPosition.x += gap;
        endPosition.y = altitude.RandomValue;
        sequenceEndX = endPosition.x + sequence;
    }
}
