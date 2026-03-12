using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class HtmlToUnityPrefab : EditorWindow
{
    private TextAsset htmlFile;
    private string prefabFolder = "Assets/GeneratedUI";
    private Sprite defaultBackground;
    private Sprite defaultButton;
    private TMP_FontAsset defaultFont;

    private List<UIElementInfo> trackedElements = new List<UIElementInfo>();

    [MenuItem("Tools/HTML to Unity Prefab")]
    public static void OpenWindow()
    {
        GetWindow<HtmlToUnityPrefab>("HTML to Unity Prefab");
    }

    private void OnGUI()
    {
        GUILayout.Label("HTML to Unity Prefab Tool", EditorStyles.boldLabel);

        htmlFile = (TextAsset)EditorGUILayout.ObjectField("HTML File", htmlFile, typeof(TextAsset), false);
        defaultBackground = (Sprite)EditorGUILayout.ObjectField("Default Background", defaultBackground, typeof(Sprite), false);
        defaultButton = (Sprite)EditorGUILayout.ObjectField("Default Button", defaultButton, typeof(Sprite), false);
        defaultFont = (TMP_FontAsset)EditorGUILayout.ObjectField("Default TMP Font", defaultFont, typeof(TMP_FontAsset), false);
        prefabFolder = EditorGUILayout.TextField("Prefab Folder", prefabFolder);

        if (GUILayout.Button("Generate UI Prefab"))
        {
            if (htmlFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Select an HTML file first!", "OK");
            }
            else
            {
                GenerateUIPrefab();
            }
        }

        if (GUILayout.Button("Attach & Wire Bindings Script"))
        {
            AttachAndWireBindingsScript();
        }
    }

    #region UI Generation

    private void GenerateUIPrefab()
    {
        trackedElements.Clear();

        if (!Directory.Exists(prefabFolder))
            Directory.CreateDirectory(prefabFolder);

        string prefabName = Path.GetFileNameWithoutExtension(htmlFile.name);
        string prefabPath = Path.Combine(prefabFolder, prefabName + ".prefab");

        if (File.Exists(prefabPath))
        {
            if (!EditorUtility.DisplayDialog("Overwrite?", $"Prefab already exists at {prefabPath}. Overwrite?", "Yes", "No"))
                return;
        }

        GameObject canvasGO = new GameObject("Canvas_" + prefabName,
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject rootPanel = CreateUIPanel("RootPanel", canvasGO.transform, defaultBackground);

        // Parse HTML and generate UI
        ParseHtmlToUI(htmlFile.text, rootPanel.transform);

        // Generate bindings script
        GenerateBindingsScript(prefabName, trackedElements);

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);
        DestroyImmediate(canvasGO);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Prefab generated at {prefabPath}\nBindings script created.", "OK");
    }

    private GameObject CreateUIPanel(string name, Transform parent, Sprite background)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(parent, false);

        Image img = panel.GetComponent<Image>();
        img.sprite = background;
        img.color = Color.white;
        img.type = Image.Type.Sliced;

        VerticalLayoutGroup layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 5;
        layout.padding = new RectOffset(5, 5, 5, 5);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return panel;
    }

    private GameObject CreateUIButton(string name, Transform parent, string text)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        Image img = buttonGO.GetComponent<Image>();
        img.sprite = defaultButton;
        img.color = Color.white;
        img.type = Image.Type.Sliced;

        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(buttonGO.transform, false);

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = defaultFont;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        trackedElements.Add(new UIElementInfo { name = name, type = "Button" });

        return buttonGO;
    }

    private GameObject CreateUIText(string name, Transform parent, string text)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.font = defaultFont;
        tmp.fontSize = 28;
        tmp.color = Color.white;

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(10, -50);
        rt.offsetMax = new Vector2(-10, 0);

        trackedElements.Add(new UIElementInfo { name = name, type = "Text" });

        return textGO;
    }

    private void ParseHtmlToUI(string html, Transform parent)
    {
        string[] lines = html.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Stack<Transform> panelStack = new Stack<Transform>();
        panelStack.Push(parent);

        foreach (var line in lines)
        {
            string trimmed = line.Trim().ToLower();
            if (trimmed.StartsWith("<button"))
            {
                string btnName = ExtractElementName(line, "Button_" + trackedElements.Count);
                CreateUIButton(btnName, panelStack.Peek(), ExtractInnerText(line));
            }
            else if (trimmed.StartsWith("<p") || trimmed.StartsWith("<span"))
            {
                string txtName = ExtractElementName(line, "Text_" + trackedElements.Count);
                CreateUIText(txtName, panelStack.Peek(), ExtractInnerText(line));
            }
            else if (trimmed.StartsWith("<div"))
            {
                string panelName = ExtractElementName(line, "Panel_" + trackedElements.Count);
                GameObject panel = CreateUIPanel(panelName, panelStack.Peek(), defaultBackground);
                panelStack.Push(panel.transform);
            }
            else if (trimmed.StartsWith("</div"))
            {
                if (panelStack.Count > 1)
                    panelStack.Pop();
            }
        }
    }

    private string ExtractInnerText(string htmlLine)
    {
        int start = htmlLine.IndexOf('>') + 1;
        int end = htmlLine.LastIndexOf('<');
        if (start >= 0 && end >= start)
        {
            return htmlLine.Substring(start, end - start);
        }
        return "";
    }

    private string ExtractElementName(string htmlLine, string defaultName)
    {
        int idIndex = htmlLine.IndexOf("id=\"", StringComparison.OrdinalIgnoreCase);
        if (idIndex >= 0)
        {
            int start = idIndex + 4;
            int end = htmlLine.IndexOf('"', start);
            if (end > start)
                return htmlLine.Substring(start, end - start);
        }

        int nameIndex = htmlLine.IndexOf("name=\"", StringComparison.OrdinalIgnoreCase);
        if (nameIndex >= 0)
        {
            int start = nameIndex + 6;
            int end = htmlLine.IndexOf('"', start);
            if (end > start)
                return htmlLine.Substring(start, end - start);
        }

        return defaultName;
    }

    #endregion

    #region Bindings Script Generation

    private void GenerateBindingsScript(string prefabName, List<UIElementInfo> elements)
    {
        string scriptFolder = Path.Combine(prefabFolder, "Scripts");
        if (!Directory.Exists(scriptFolder))
            Directory.CreateDirectory(scriptFolder);

        string className = prefabName + "Bindings";
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using TMPro;");
        sb.AppendLine("using UnityEngine.Events;");
        sb.AppendLine("");
        sb.AppendLine($"public class {className} : MonoBehaviour");
        sb.AppendLine("{");

        foreach (var elem in elements)
        {
            string unityType = elem.type switch
            {
                "Button" => "Button",
                "Text" => "TextMeshProUGUI",
                _ => "GameObject"
            };
            sb.AppendLine($"    public {unityType} {elem.name};");
        }

        sb.AppendLine("");
        sb.AppendLine("    // UnityEvents for external scripts");
        foreach (var elem in elements)
        {
            if (elem.type == "Button")
                sb.AppendLine($"    public UnityEvent On{elem.name}Clicked;");
        }

        sb.AppendLine("");
        sb.AppendLine("    private void Awake()");
        sb.AppendLine("    {");
        foreach (var elem in elements)
        {
            if (elem.type == "Button")
            {
                sb.AppendLine($"        if ({elem.name} != null) {elem.name}.onClick.AddListener(() => On{elem.name}Clicked?.Invoke());");
            }
        }
        sb.AppendLine("    }");

        sb.AppendLine("}");

        string path = Path.Combine(scriptFolder, className + ".cs");
        File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();
    }

    private void AttachAndWireBindingsScript()
    {
        string prefabPath = EditorUtility.OpenFilePanel("Select Prefab to Attach Bindings", prefabFolder, "prefab");
        if (string.IsNullOrEmpty(prefabPath))
            return;

        prefabPath = "Assets" + prefabPath.Substring(Application.dataPath.Length);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Prefab not found!", "OK");
            return;
        }

        // Find generated bindings script
        string scriptName = Path.GetFileNameWithoutExtension(prefabPath) + "Bindings";
        MonoScript bindingsScript = (MonoScript)AssetDatabase.LoadAssetAtPath(
            Path.Combine(prefabFolder, "Scripts", scriptName + ".cs").Replace("\\", "/"),
            typeof(MonoScript));

        if (bindingsScript == null)
        {
            EditorUtility.DisplayDialog("Error", "Bindings script not found!", "OK");
            return;
        }

        var component = prefab.GetComponent(bindingsScript.GetClass());
        if (component == null)
            component = prefab.AddComponent(bindingsScript.GetClass());

        var fields = component.GetType().GetFields();
        foreach (var field in fields)
        {
            Transform t = prefab.transform.Find(field.Name);
            if (t != null)
            {
                object value = null;
                if (field.FieldType == typeof(Button))
                    value = t.GetComponent<Button>();
                else if (field.FieldType == typeof(TextMeshProUGUI))
                    value = t.GetComponent<TextMeshProUGUI>();
                else if (field.FieldType == typeof(GameObject))
                    value = t.gameObject;

                if (value != null)
                    field.SetValue(component, value);
            }
        }

        EditorUtility.SetDirty(prefab);
        EditorUtility.DisplayDialog("Success", "Bindings script attached and wired!", "OK");
    }

    #endregion

    private class UIElementInfo
    {
        public string name;
        public string type;
    }
}