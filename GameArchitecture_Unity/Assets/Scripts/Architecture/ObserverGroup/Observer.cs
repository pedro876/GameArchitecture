using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architecture.ObserverGroup;
using UnityEditor;

public abstract class Observer<TEnum> : MonoBehaviour, IObserver<TEnum> where TEnum : System.Enum
{
    [SerializeField] bool subscribedWhileDisabled = false;
    [SerializeField] bool subscribeAll = true;
    [SerializeField] List<TEnum> eventsToSubscribe = new List<TEnum>();

    public string GetName() => name;
    public abstract void Notify(TEnum evt, object evtInfo);
    private bool subscribed = false;

    private void Awake()
    {
        Subscribe();
        DoAwake();
    }

    private void OnEnable()
    {
        Subscribe();
        DoOnEnable();
    }

    private void OnDisable()
    {
        if(!subscribedWhileDisabled)
            Unsubscribe();
        DoOnDisable();
    }

    private void OnDestroy()
    {
        if(!subscribedWhileDisabled)
            Unsubscribe();
        DoOnDestroy();
    }

    protected virtual void DoAwake() { }
    protected virtual void DoOnEnable() { }
    protected virtual void DoOnDisable() { }
    protected virtual void DoOnDestroy() { }

    private void Subscribe()
    {
        if (subscribed) return;
        subscribed = true;
        if (subscribeAll)
            ObserverGroup<TEnum>.SubscribeAll(this);
        else
            ObserverGroup<TEnum>.Subscribe(this, eventsToSubscribe);
    }

    private void Unsubscribe()
    {
        if(!subscribed) return;
        subscribed = false;
        if (subscribeAll)
            ObserverGroup<TEnum>.UnsubscribeAll(this);
        else
            ObserverGroup<TEnum>.Unsubscribe(this, eventsToSubscribe);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Observer<>), true)]
[CanEditMultipleObjects]
public class ObserverEditor : Editor
{
    SerializedProperty subscribeAll;
    SerializedProperty eventsToSubscribe;

    private GUIStyle backgroundStyle;
    private GUIStyle textStyle;
    private Texture2D texture;

    void OnEnable()
    {
        subscribeAll = serializedObject.FindProperty("subscribeAll");
        eventsToSubscribe = serializedObject.FindProperty("eventsToSubscribe");
        
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty iterator = serializedObject.GetIterator();
        iterator.NextVisible(true);
        using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
        {
            EditorGUILayout.PropertyField(iterator, true);
        }

        WriteHeader("OBSERVER");

        bool wasEventsToSubscribe = false;

        while (iterator.NextVisible(false))
        {
            if (wasEventsToSubscribe)//and there are more properties
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                WriteHeader("OTHER PROPERTIES");
            }

            wasEventsToSubscribe = iterator.propertyPath == eventsToSubscribe.propertyPath;
            if (wasEventsToSubscribe && subscribeAll.boolValue)
                continue;
            EditorGUILayout.PropertyField(iterator, true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void WriteHeader(string text)
    {
        CreateHeaderBackgroundStyle();
        CreateHeaderLabelStyle();
        GUILayout.BeginHorizontal(backgroundStyle);
        GUILayout.Label(text, textStyle);
        GUILayout.EndHorizontal();
        return;

        void CreateHeaderBackgroundStyle()
        {
            if (backgroundStyle != null) return;
            backgroundStyle = new GUIStyle();
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(.12f, .12f, .12f));
            texture.Apply();
            backgroundStyle.normal.background = texture;
        }

        void CreateHeaderLabelStyle()
        {
            if (textStyle != null) return;
            textStyle = GUI.skin.GetStyle("Label");
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontStyle = FontStyle.Bold;
        }
    }

    
}
#endif