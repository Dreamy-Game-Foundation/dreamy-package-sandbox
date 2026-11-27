using System;
using System.Collections;
using System.Collections.Generic;
using ThunderPunch.Debugger;
using ThunderPunch.Debugger.Presentation;
using ThunderPunch.Debugger.SceneInspection;
using UnityEngine;

namespace Dreamy.Template.Debugging
{
    public sealed class HierarchyInspectorLab : MonoBehaviour
    {
        [Flags]
        public enum Channels { Red = 1, Green = 2, Blue = 4 }
        public enum PreviewMode { Idle, Rotate, Pulse }

        public GameObject Samples;
        public GameObject InitialSelection;
        public Rigidbody Body;
        public Rigidbody2D Body2D;
        public Transform GroupA, GroupB;
        public AudioSource Audio;
        public bool Animate;
        public PreviewMode Mode;
        public Channels Mask = Channels.Red | Channels.Blue;
        public long LargeInteger = 9007199254740993L;
        public float SignedValue = -1.25f;
        public Vector4 FourAxes = new Vector4(1, 2, 3, 4);
        public Color Tint = new Color(.8f, .35f, .25f);
        public List<int> Values = new List<int> { 1, 2, 3 };
        public GameObject Reference;

        private DebuggerRuntime debugger;
        private GameObject originalSamples, resetHolder, spawned;
        private AudioClip tone;
        private float initialMass;
        private Vector3 initialPosition;
        private bool revealPending;

        private IEnumerator Start()
        {
            debugger = DebuggerRuntime.Instance;
            if (debugger == null) yield break;
            initialMass = Body.mass;
            initialPosition = Body.position;
            // Keep a reset template under the debugger-owned root, outside the inspected tree.
            resetHolder = new GameObject("Lab reset template");
            resetHolder.SetActive(false);
            resetHolder.transform.SetParent(debugger.transform, false);
            originalSamples = Instantiate(Samples, resetHolder.transform, false);
            revealPending = true;
            OpenInspector();
            while (revealPending && debugger != null)
            {
                if (debugger.CanInspectScene)
                {
                    var view = FindFirstObjectByType<SceneTabController>();
                    if (view != null && view.Reveal(InitialSelection)) revealPending = false;
                }
                yield return null;
            }
        }

        private void OnGUI()
        {
            if (debugger == null) return;
            var width = Mathf.Min(310, Screen.width - 24);
            var area = new Rect(12, 12, width, 238);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("Hierarchy Inspector Lab");
            if (GUILayout.Button("Open Hierarchy", GUILayout.Height(30))) OpenInspector();
            if (Body != null)
                GUILayout.Label($"Mass {initialMass:G} → {Body.mass:G}   ·   Position X {initialPosition.x:G} → {Body.position.x:G}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Simulate")) { Body.isKinematic = false; Body.useGravity = true; Body2D.simulated = true; }
            if (GUILayout.Button("Play tone")) PlayTone();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spawn")) Spawn();
            if (GUILayout.Button("Reparent") && spawned != null)
                spawned.transform.SetParent(spawned.transform.parent == GroupA ? GroupB : GroupA, true);
            if (GUILayout.Button("Destroy") && spawned != null) Destroy(spawned);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Reset samples")) ResetSamples();
            GUILayout.EndArea();
        }

        private void OpenInspector() => debugger.Open(DefaultTabs.Scene);
        private void Update() { if (Animate) SignedValue = Mathf.Sin(Time.unscaledTime); }

        private void Spawn()
        {
            if (spawned != null) return;
            spawned = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spawned.name = "Spawned sphere";
            spawned.transform.SetParent(GroupA, false);
            spawned.transform.localPosition = new Vector3(2, 1, 0);
        }

        private void PlayTone()
        {
            if (tone == null)
            {
                const int rate = 22050;
                var data = new float[rate / 4];
                for (var i = 0; i < data.Length; i++)
                    data[i] = .15f * Mathf.Sin(2 * Mathf.PI * 440 * i / rate) * (1 - (float)i / data.Length);
                tone = AudioClip.Create("Inspector Lab Tone", data.Length, 1, rate, false);
                tone.SetData(data, 0);
            }
            Audio.PlayOneShot(tone);
        }

        private void ResetSamples()
        {
            if (originalSamples == null) return;
            var bodyPath = Body.name;
            var body2DPath = Body2D.name;
            var groupAName = GroupA.name;
            var groupBName = GroupB.name;
            var audioName = Audio.name;
            var old = Samples;
            old.SetActive(false);
            Samples = Instantiate(originalSamples);
            Samples.name = old.name;
            Destroy(old);
            Body = Samples.GetComponentsInChildren<Rigidbody>(true).FindNamed(bodyPath);
            Body2D = Samples.GetComponentsInChildren<Rigidbody2D>(true).FindNamed(body2DPath);
            GroupA = Samples.GetComponentsInChildren<Transform>(true).FindNamed(groupAName);
            GroupB = Samples.GetComponentsInChildren<Transform>(true).FindNamed(groupBName);
            Audio = Samples.GetComponentsInChildren<AudioSource>(true).FindNamed(audioName);
            InitialSelection = Reference = Body.gameObject;
            Animate = false; Mode = PreviewMode.Idle; Mask = Channels.Red | Channels.Blue;
            LargeInteger = 9007199254740993L; SignedValue = -1.25f;
            FourAxes = new Vector4(1, 2, 3, 4); Tint = new Color(.8f, .35f, .25f);
            Values = new List<int> { 1, 2, 3 };
            FindFirstObjectByType<SceneTabController>()?.Reveal(InitialSelection);
        }
        private void OnDestroy()
        {
            if (tone != null) Destroy(tone);
            if (resetHolder != null) Destroy(resetHolder);
        }
    }

    internal static class HierarchyLabLookup
    {
        internal static T FindNamed<T>(this T[] components, string name) where T : Component =>
            Array.Find(components, component => component.name == name);
    }
}
