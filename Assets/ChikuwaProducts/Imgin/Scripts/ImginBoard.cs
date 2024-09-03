
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
        private Vector2[] _maxScales = Array.Empty<Vector2>();

        void Start()
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            _panels = new GameObject[renderers.Length];
            _maxScales = new Vector2[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                var textures = new RenderTexture(MAX_SCREEN_SIZE, MAX_SCREEN_SIZE, 0);
                _maxScales[i] = new Vector2(renderer.transform.localScale.x, renderer.transform.localScale.y);

                renderer.material.mainTexture = textures;
                renderer.enabled = false;

                _panels[i] = renderer.gameObject;
            }
            if (ImginLoader != null)
            {
                ImginLoader.AddBoard(this);
            }
        }

        public void ApplyImage(Texture texture, int index, float ratio)
        {
            index -= (int)Offset;

            if (index < 0 || index >= _panels.Length) return;

            var renderer = _panels[index].GetComponent<MeshRenderer>();

            VRCGraphics.Blit(texture, (RenderTexture)renderer.material.mainTexture);

            var scale = _panels[index].transform.localScale;
            var maxScale = _maxScales[index];
            var hv = ratio < 1f;
            scale.x = hv ? maxScale.x : maxScale.x / ratio;
            scale.y = hv ? maxScale.y * ratio : maxScale.y;

            _panels[index].transform.localScale = scale;

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