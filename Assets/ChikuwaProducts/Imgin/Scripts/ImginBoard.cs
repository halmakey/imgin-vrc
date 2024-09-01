
using UdonSharp;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using System;

namespace Chikuwa.Imgin
{
    public class ImginBoard : UdonSharpBehaviour
    {
        readonly int MAX_SCREEN_SIZE = 1280;
        public ImginLoader ImginLoader;
        public uint Offset;

        private GameObject[] _panels = Array.Empty<GameObject>();
        private float[] _maxScales = Array.Empty<float>();

        void Start()
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            _panels = new GameObject[renderers.Length];
            _maxScales = new float[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                var textures = new RenderTexture(MAX_SCREEN_SIZE, MAX_SCREEN_SIZE, 0);
                _maxScales[i] = Mathf.Min(renderer.transform.localScale.x, renderer.transform.localScale.y);

                renderer.material.mainTexture = textures;
                renderer.enabled = false;
                renderer.transform.localScale = Vector3.zero;

                _panels[i] = renderer.gameObject;
            }
            if (ImginLoader != null)
            {
                ImginLoader.AddBoard(this);
            }
        }

        public void ApplyAspectRatios(float[] ratios)
        {
            for (int i = 0; i < Math.Min(_panels.Length, ratios.Length - Offset); i++)
            {
                var ratio = ratios[Offset + i];
                var scale = _panels[i].transform.localScale;
                var maxScale = _maxScales[i];
                if (ratio < 1f)
                {
                    scale.x = maxScale;
                    scale.y = maxScale * ratio;
                }
                else
                {
                    scale.x = maxScale / ratio;
                    scale.y = maxScale;
                }
                _panels[i].transform.localScale = scale;
            }
        }

        public void ApplyImage(Material material, int index)
        {
            index -= (int)Offset;
            if (index < 0 || index >= _panels.Length) return;
            var renderer = _panels[index].GetComponent<MeshRenderer>();
            VRCGraphics.Blit(material.mainTexture, (RenderTexture)renderer.material.mainTexture, material);
            renderer.enabled = true;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.blue;

            var renderers = GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Handles.Label(renderers[i].transform.position, ("#" + (Offset + i + 1)).ToString(), style);
            }
        }
#endif
    }
}