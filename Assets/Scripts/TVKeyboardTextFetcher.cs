using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TVKeyboardTextFetcher : MonoBehaviour
{

    public event Action<string> OnResult;
    // Store current keyboard text for Apply logic
    private string keyboardText = "";



  
    public void RequestAndroidTVKeyboard()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        ShowAndroidTVKeyboard(keyboardText, (string result) =>
        {
            keyboardText = result;      // store for Apply logic
            OnResult?.Invoke(result);
        });
#endif
    }

    // Call this when your Apply button is pressed
    public void ApplyInput()
    {
        // Here you can run your actual logic with keyboardText
        Debug.Log("Applied text: " + keyboardText);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void ShowAndroidTVKeyboard(string initialText, Action<string> callback)
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject editText = new AndroidJavaObject("android.widget.EditText", activity);
                editText.Call("setText", initialText);

                AndroidJavaObject builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity);
                builder.Call<AndroidJavaObject>("setView", editText);
                builder.Call<AndroidJavaObject>("setTitle", "Enter text");

                // OK button
                builder.Call<AndroidJavaObject>("setPositiveButton", "OK",
                    new OnClickListenerProxy((dialog, which) =>
                    {
                        try
                        {
                            AndroidJavaObject editable = editText.Call<AndroidJavaObject>("getText");
                            string text = editable.Call<string>("toString");

                            // update InputField safely
                            callback?.Invoke(text);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Error reading EditText: " + e);
                        }
                    })
                );

                // Cancel button
                builder.Call<AndroidJavaObject>("setNegativeButton", "Cancel",
                    new OnClickListenerProxy((dialog, which) =>
                    {
                        callback?.Invoke(initialText);
                    })
                );

                AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
                dialog.Call("show");
            }));
        }
    }

    private class OnClickListenerProxy : AndroidJavaProxy
    {
        private readonly Action<AndroidJavaObject, int> callback;

        public OnClickListenerProxy(Action<AndroidJavaObject, int> callback)
            : base("android.content.DialogInterface$OnClickListener")
        {
            this.callback = callback;
        }

        void onClick(AndroidJavaObject dialog, int which)
        {
            callback?.Invoke(dialog, which);
        }
    }
#endif
}