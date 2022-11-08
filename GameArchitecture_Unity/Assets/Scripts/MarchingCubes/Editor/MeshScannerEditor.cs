using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.ComponentModel;
using UnityEditor.PackageManager.UI;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine.Rendering;

[CustomEditor(typeof(MeshScanner))]
public class MeshScannerEditor : Editor
{
    MeshScanner scanner;
    Texture2D texture;
    int textureZIndex = 0;

    bool scanning = false;
    bool cancelling = false;
    float pct = 0f;

    private void OnEnable()
    {
        scanner = (target as MeshScanner);
        textureZIndex = scanner.finalResolution / 2;
    }

    private void OnDisable()
    {
        cancelling = true;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        textureZIndex = EditorGUILayout.IntSlider("Z index", textureZIndex, 0, scanner.finalResolution - 1);
        bool showScan = !scanning || cancelling;
        GUI.enabled = showScan;
        if (GUILayout.Button(!showScan ? $"{pct*100}%" : "Scan"))
        {
            cancelling = false;
            scanner.Recalculate();
            Scan();
        }
        GUI.enabled = true;
        if (GUILayout.Button("Cancel"))
        {
            cancelling = true;
        }
        
        
        if(texture != null)
        {
            //Texture2D myTexture = AssetPreview.GetAssetPreview(object);

            GUILayout.Label(texture, GUILayout.ExpandHeight(true));
        }
    }

    private async void Scan()
    {
        const int maxMilliseconds = 100;
        const int waitMilliseconds = 1;//(int)((1000f/60f) - maxMilliseconds);
        DateTime init = DateTime.Now;
        scanning = true;
        texture = new Texture2D(scanner.finalResolution, scanner.finalResolution);
        float iterations = scanner.finalResolution * scanner.finalResolution;
        for (int y = 0; y < scanner.finalResolution && !cancelling; y++)
        {
            for (int x = 0; x < scanner.finalResolution && !cancelling; x++)
            {
                float posX = scanner.startPos.x + x * scanner.step + scanner.threshold;
                float posY = scanner.startPos.y + y * scanner.step + scanner.threshold;
                float posZ = scanner.startPos.z + textureZIndex * scanner.step + scanner.threshold;
                Vector3 pos = new Vector3(posX, posY, posZ);
                bool inMesh = scanner.PointInMesh(pos, out Vector3 firstHit);
                texture.SetPixel(x, y, inMesh ? Color.black : Color.white);
                
                DateTime now = DateTime.Now;
                if((now-init).TotalMilliseconds >= maxMilliseconds)
                {
                    pct = (y * scanner.finalResolution + x) / iterations;
                    init = now;
                    //texture.Apply();
                    await Task.Delay(waitMilliseconds);
                }
            }
        }
        if(!cancelling)
            texture.Apply();
        scanning = false;
        cancelling = false;
    }
}
