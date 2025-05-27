using UnityEngine;

[System.Serializable]
public struct GripData
{
    public Vector3 position;
    public Vector3 normal;

    public GripData(Vector3 position, Vector3 normal)
    {
        this.position = position;
        this.normal = normal;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is GripData))
            return false;
        GripData other = (GripData)obj;
        return position == other.position && normal == other.normal;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode() ^ normal.GetHashCode();
    }

    public static bool operator ==(GripData a, GripData b)
    {
        return a.position == b.position && a.normal == b.normal;
    }

    public static bool operator !=(GripData a, GripData b)
    {
        return !(a == b);
    }
}
