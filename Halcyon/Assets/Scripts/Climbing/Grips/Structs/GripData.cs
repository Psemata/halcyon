using UnityEngine;

[System.Serializable]
public struct GripData
{
    public Vector3 position;
    public Vector3 normal;
    public bool NearPath;

    public GripData(Vector3 position, Vector3 normal, bool nearPath = false)
    {
        this.position = position;
        this.normal = normal;
        this.NearPath = nearPath;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is GripData))
            return false;
        GripData other = (GripData)obj;
        return position == other.position && normal == other.normal && NearPath == other.NearPath;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode() ^ normal.GetHashCode();
    }

    public static bool operator ==(GripData a, GripData b)
    {
        return a.position == b.position && a.normal == b.normal && a.NearPath == b.NearPath;
    }

    public static bool operator !=(GripData a, GripData b)
    {
        return !(a == b);
    }
}
