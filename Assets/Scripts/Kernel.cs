using UnityEngine;

public static class Kernel
{
    private static float POLYCONST = 315 / (64 * Mathf.PI);
    private static float POLYGRADCONST = -945 / (32 * Mathf.PI);
    private static float POLYLAPCONST = -945 / (32 * Mathf.PI);

    public static float Poly6(Vector3 distance, float h)
    {

        if (distance.magnitude > h || distance.magnitude < 0)
            return 0;

        return POLYCONST * Mathf.Pow(h, -9) * Mathf.Pow(Mathf.Pow(h, 2) - Mathf.Pow(distance.magnitude, 2), 3);

    }

    public static Vector3 Poly6Grad(Vector3 distance, float h)
    {
        if (distance.magnitude > h || distance.magnitude < 0)
            return Vector3.zero;

        return POLYGRADCONST * Mathf.Pow(h, -9) * distance * Mathf.Pow(Mathf.Pow(h, 2) 
        - Mathf.Pow(distance.magnitude, 2), 2);
    }

    public static float Poly6Lap(Vector3 distance, float h)
    {
        if (distance.magnitude > h || distance.magnitude < 0)
            return 0;

        return POLYLAPCONST * (Mathf.Pow(h, -9) * (Mathf.Pow(h, 2)
        - Mathf.Pow(distance.magnitude, 2)) * (3 * Mathf.Pow(h, 2)
        - 7 * Mathf.Pow(distance.magnitude, 2)));
    }

    public static Vector3 SpikyGrad(Vector3 distance, float h)
    {
        float distSqr = distance.sqrMagnitude;
        if (distSqr >= 0 && distSqr <= h * h)
        {
            if (distSqr < 0.01f)
            {
                return -45 / (Mathf.PI * Mathf.Pow(h, 6)) * distance.normalized;
            }
            return -45 / (Mathf.PI * Mathf.Pow(h, 6)) * Mathf.Pow(h - distance.magnitude, 2) * distance.normalized;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public static float ViscosityLap(Vector3 distance, float h)
    {
        float distSqr = distance.sqrMagnitude;
        if (distSqr >= 0 && distSqr <= h * h)
        {
            return 45 / (Mathf.PI * Mathf.Pow(h, 6)) * (h - distance.magnitude);
        }
        else
        {
            return 0.0f;
        }
    }
}
