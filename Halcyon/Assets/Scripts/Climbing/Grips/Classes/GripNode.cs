using System.Collections.Generic;

public class GripNode
{
    public GripData data;
    public List<GripNode> neighbors = new List<GripNode>();
    public int index;

    public GripNode(GripData data, int index)
    {
        this.data = data;
        this.index = index;
    }
}
