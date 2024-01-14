using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual Interactable OnClick()
    {
        return this;
    }
}
