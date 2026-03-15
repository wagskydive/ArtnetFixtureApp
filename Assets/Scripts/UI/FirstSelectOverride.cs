using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class FirstSelectOverride : MonoBehaviour
{
    void OnEnable()
    {
        Selectable selectable = GetComponent<Selectable>();
        if(selectable == null)
        {
            Debug.Log("No Selectable Object is attached to the game object, so selection is not possible");
            return;
        }
        selectable.Select();        
    }


}
