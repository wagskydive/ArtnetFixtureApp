using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FirstSelectOverride))]
public class FirstSelectOverrideEditor : Editor
{
    private void OnEnable()
    {
        var selector = target as FirstSelectOverride;
        Transform currentTransform = selector.transform.parent;

        // Traverse up the hierarchy to find a Canvas
        Canvas canvasParent = null;
        while (currentTransform != null)
        {
            canvasParent = currentTransform.GetComponent<Canvas>();
            if (canvasParent != null)
                break;
            currentTransform = currentTransform.parent;
        }

        // If no Canvas found in hierarchy, remove the component
        if (canvasParent == null)
        {
            EditorUtility.DisplayDialog("FirstSelectOverride Error", 
                "FirstSelectOverride can only be added to objects within a Canvas hierarchy.", 
                "OK");
            DestroyImmediate(selector);
            return;
        }

        // Check if another FirstSelectOverride exists under the same Canvas (including all children)
        var othersInCanvas = canvasParent.GetComponentsInChildren<FirstSelectOverride>();
        if (othersInCanvas.Length > 1)
        {
            // Find the other selector to show which object already has it
            FirstSelectOverride existingSelector = null;
            foreach (var other in othersInCanvas)
            {
                if (other != selector)
                {
                    existingSelector = other;
                    break;
                }
            }

            string existingObjectName = existingSelector != null ? existingSelector.gameObject.name : "Unknown";
            
            EditorUtility.DisplayDialog("FirstSelectOverride Error", 
                $"FirstSelectOverride can only exist once per Canvas hierarchy.\n\n" +
                $"Canvas: '{canvasParent.gameObject.name}'\n" +
                $"Already exists on: '{existingObjectName}'", 
                "OK");
            DestroyImmediate(selector);
        }
    }
}