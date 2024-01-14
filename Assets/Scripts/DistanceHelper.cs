using UnityEngine;

public static class DistanceHelper
{
    public static float DistanceThreshold = 0.1f;
    public static bool IsWithinDistance(Vector3 subjectPosition, Vector3 targetPosition)
    {
        float currentDistance = Vector3.Distance(subjectPosition, targetPosition);
        return currentDistance <= DistanceThreshold;
    }
    public static bool IsWithinDistance(Vector3 subjectPosition, Vector3 targetPosition, float distanceThreshold)
    {
        float currentDistance = Vector3.Distance(subjectPosition, targetPosition);
        return currentDistance <= distanceThreshold;
    }
}
