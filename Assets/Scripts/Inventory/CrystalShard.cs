using UnityEngine;

public class CrystalShard : MonoBehaviour
{
    public enum Kind { Snow, Desert, Volcano }

    public string ItemName { get; private set; }

    public void SetKind(Kind kind)
    {
        ItemName = kind + " Crystal Shard";
    }
}
