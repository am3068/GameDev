using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public string name;
    public enum Status
    {
        SUCCESS,
        RUNNING,
        FAILURE
    };
    public Status status;
    public List<Node> children = new List<Node>();
    public int currentChild = 0;

    public Node() { }
    
    public Node(string name) {
        this.name = name;
    }
    
    public virtual Status Process()
    {
        return Status.SUCCESS;
    }

    public void AddChild(Node node)
    {
        children.Add(node);
    }
}
