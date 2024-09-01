
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

        private RenderTexture[] _textures = Array.Empty<RenderTexture>();

        void Start()
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            _textures = new RenderTexture[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                _textures[i] = new RenderTexture(MAX_SCREEN_SIZE, MAX_SCREEN_SIZE, 0);
                renderers[i].material.mainTexture = _textures[i];
            }
            if (ImginLoader != null)
            {
                ImginLoader.AddBoard(this);
            }
        }

        public void Blit(Texture source, int index, Material mat)
        {
            index -= (int)Offset;
            if (index < 0 || index >= _textures.Length) return;
            VRCGraphics.Blit(source, _textures[index], mat);
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