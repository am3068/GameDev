using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : Node
{
    public BehaviourTree()
    {
        name = "BehaviourTree";
    }
    
    public BehaviourTree(string name)
    {
        this.name = name;
    }
}
