using System;
using System.IO;
using System.Linq;
using Dreamy.EditorTools.Capture;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class CaptureSmoke
{
    const string Key = "DreamyCaptureSmokeStage";
    const string Output = "/tmp/dreamy-capture-smoke-output";
    static double next;
    static double limit;
    static int originalFiles;
    static Texture2D previousPreview;

    static CaptureSmoke()
    {
        if (!Environment.GetCommandLineArgs().Contains("-dreamyCaptureSmoke")) return;
        EditorApplication.update += Update;
        limit = EditorApplication.timeSinceStartup + 90;
        next = EditorApplication.timeSinceStartup + 3;
    }
    static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Update()
    {
        if (EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
        try
        {
            if (EditorApplication.timeSinceStartup > limit) throw new Exception("Smoke deadline; " + GameViewCapture.Status);
            int stage = SessionState.GetInt(Key, 0);
            if (stage == 0)
            {
                EditorSettings.enterPlayModeOptionsEnabled = Environment.GetCommandLineArgs().Contains("-dreamyNoReload");
                EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
                Directory.CreateDirectory(Output);
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camera = new GameObject("Capture Fixture Camera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(1,0,1,1);
                camera.allowHDR = false;
                var canvas = new GameObject("Overlay Canvas", typeof(Canvas)).GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var image = new GameObject("Green Marker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                image.transform.SetParent(canvas.transform, false);
                image.color = Color.green;
                image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0,1);
                image.rectTransform.pivot = new Vector2(0,1);
                image.rectTransform.anchoredPosition = new Vector2(10,-10);
                image.rectTransform.sizeDelta = new Vector2(100,60);
                EditorSceneManager.SaveScene(scene, "Assets/CaptureFixture.unity");
                SessionState.SetInt(Key, 1);
                EditorApplication.ExecuteMenuItem("Window/General/Game");
                EditorApplication.EnterPlaymode();
                next = EditorApplication.timeSinceStartup + 3;
                return;
            }
            if (!EditorApplication.isPlaying) return;
            if (stage == 1)
            {
                Require(GameViewCapture.GetUnavailableReason(CaptureMode.Copy) == null,
                    GameViewCapture.GetUnavailableReason(CaptureMode.Copy));
                GameViewCapture.Start(CaptureMode.Save, 0, Output);
                SessionState.SetInt(Key, 2);
                return;
            }
            if (GameViewCapture.IsBusy) return;
            if (stage == 2)
            {
                Require(GameViewCapture.LastResult != null && GameViewCapture.LastResult.SavedPath != null, "Save failed: " + GameViewCapture.Status);
                var tex = GameViewCapture.Preview;
                Require(tex != null && tex.width > 120 && tex.height > 100, "No preview");
                var center = tex.GetPixel(tex.width/2, tex.height/2);
                var marker = tex.GetPixel(30, tex.height-30);
                Require(center.r > .8f && center.b > .8f && center.g < .2f, "Background pixels incorrect: "+center);
                Require(marker.g > .8f && marker.r < .2f && marker.b < .2f, "Overlay UI missing: "+marker);
                originalFiles = Directory.GetFiles(Output).Length;
                SessionState.SetInt("CaptureFiles",originalFiles);
                GameViewCapture.OutputPreview(CaptureMode.Copy, "/invalid-do-not-create");
                SessionState.SetInt(Key, 3);
                return;
            }
            if (stage == 3)
            {
                Require(GameViewCapture.LastResult != null && GameViewCapture.LastResult.Copied, "Copy failed: " + GameViewCapture.Status);
                Require(GameViewCapture.LastResult.SavedPath == null, "Copy wrote a file");
                Require(Directory.GetFiles(Output).Length == SessionState.GetInt("CaptureFiles",0), "Copy changed files");
                GameViewCapture.OutputPreview(CaptureMode.CopyAndSave, Output);
                SessionState.SetInt(Key, 4);
                return;
            }
            if (stage == 4)
            {
                Require(GameViewCapture.LastResult != null && GameViewCapture.LastResult.Copied && GameViewCapture.LastResult.SavedPath != null,
                    "Copy+Save failed: " + GameViewCapture.Status);
                SessionState.SetInt("CaptureFiles",Directory.GetFiles(Output).Length);
                Time.timeScale = 0;
                GameViewCapture.Start(CaptureMode.Save, 3, Output);
                SessionState.SetInt(Key, 5);
                return;
            }
            if (stage == 5)
            {
                Require(GameViewCapture.LastResult != null && GameViewCapture.LastResult.SavedPath != null, "timeScale=0 capture failed: " + GameViewCapture.Status);
                Require(Directory.GetFiles(Output).Length == SessionState.GetInt("CaptureFiles",0)+1,"Missing timescale capture");
                Time.timeScale = 1;
                SessionState.SetInt("CaptureFiles",Directory.GetFiles(Output).Length);
                GameViewCapture.Start(CaptureMode.Save, 5, Output);
                GameViewCapture.Start(CaptureMode.Save, 0, Output);
                Require(GameViewCapture.Status.Contains("already"), "Duplicate was not rejected");
                GameViewCapture.Cancel();
                SessionState.SetInt(Key, 6);
                return;
            }
            if (stage == 6)
            {
                Require(Directory.GetFiles(Output).Length == SessionState.GetInt("CaptureFiles",0), "Cancelled capture wrote a file");
                Require(!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty,"Scene became dirty");
                DreamyScreenshotWindow.Open();
                next = EditorApplication.timeSinceStartup + 2;
                SessionState.SetInt(Key,7);
                return;
            }
            if (stage == 7)
            {
                SessionState.SetInt("CaptureFiles",Directory.GetFiles(Output).Length);
                SessionState.SetInt("CaptureWidth",GameViewCapture.Preview.width);
                SessionState.SetInt("CaptureHeight",GameViewCapture.Preview.height);
                GameViewCapture.Start(CaptureMode.Save, 0, Output);
                EditorApplication.isPaused = true;
                SessionState.SetInt(Key, 8);
                return;
            }
            if (stage == 8)
            {
                EditorApplication.isPaused = false;
                Require(GameViewCapture.Status.StartsWith("Timed out"), "Expected timeout while paused after capture start: " + GameViewCapture.Status);
                Require(Directory.GetFiles(Output).Length == SessionState.GetInt("CaptureFiles",0), "Timed out job wrote file");
                SessionState.SetInt("RepeatCapture",0);
                previousPreview = GameViewCapture.Preview;
                GameViewCapture.Start(CaptureMode.Save,0,Output);
                SessionState.SetInt(Key,9);
                return;
            }
            if (stage == 9)
            {
                Require(GameViewCapture.LastResult != null && GameViewCapture.LastResult.SavedPath != null,"Repeated capture failed");
                Require(previousPreview == null,"Old preview texture was not destroyed");
                int repeated = SessionState.GetInt("RepeatCapture",0)+1;
                SessionState.SetInt("RepeatCapture",repeated);
                if (repeated < 50)
                {
                    previousPreview = GameViewCapture.Preview;
                    GameViewCapture.Start(CaptureMode.Save,0,Output);
                    return;
                }
                Require(Resources.FindObjectsOfTypeAll<Texture2D>().Count(x => x.hideFlags == HideFlags.HideAndDontSave && x == GameViewCapture.Preview) == 1,"Preview ownership failed");
                SessionState.SetInt(Key,10);
                return;
            }
            if (stage == 10)
            {
                File.WriteAllText(Output+"/receipt.json", "{\"result\":\"passed\",\"unity\":\""+Application.unityVersion+"\",\"width\":"+GameViewCapture.Preview.width+",\"height\":"+GameViewCapture.Preview.height+",\"checks\":[\"save\",\"UI overlay pixels\",\"copy only no file\",\"copy+save\",\"timeScale zero\",\"duplicate\",\"cancel\",\"scene clean\",\"window opens\",\"paused frame timeout\",\"50 sequential captures\"]}");
                SessionState.SetInt(Key,11);
                EditorApplication.Exit(0);
            }
        }
        catch(Exception ex)
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output+"/failure.txt",ex.ToString());
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }
}
