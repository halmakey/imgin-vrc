
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Video.Components.AVPro;
using System;

namespace Chikuwa.Imgin
{
    public class ImginLoader : UdonSharpBehaviour
    {
        readonly int MAX_SCREEN_SIZE = 1280;
        readonly float TIME_OFFSET = -0.05f;
        readonly float FRAMERATE = 4f;
        public MeshRenderer BackScreen;
        public VRCUrl VideoURL;
        public float DelaySeconds;

        private VRCAVProVideoPlayer _backPlayer;
        private RenderTexture _backScreen;
        private ImginBoard[] _boards = Array.Empty<ImginBoard>();
        private int _lastReadIndex;

        void Start()
        {
            _backPlayer = GetComponent<VRCAVProVideoPlayer>();
            _backScreen = new RenderTexture(MAX_SCREEN_SIZE, MAX_SCREEN_SIZE, 0);

            _backPlayer.Stop();
            _backPlayer.EnableAutomaticResync = false;
            _backPlayer.Loop = false;
            BackScreen.material.mainTexture = _backScreen;

            SendCustomEventDelayedSeconds("StartLoad", DelaySeconds);
        }

        public void StartLoad()
        {
            _lastReadIndex = -1;
            _backPlayer.PlayURL(VideoURL);
        }

        public void AddBoard(ImginBoard board)
        {
            _boards = ArrayUtils.Append(_boards, board);
        }

        public override void OnVideoStart()
        {
            Debug.Log("ImginLoader: Load started.");
            _lastReadIndex = -1;
        }
        public override void OnVideoEnd()
        {
            Debug.Log("ImginLoader: Load completed.");
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!_backPlayer.IsPlaying)
            {
                return;
            }

            var index = (int)Math.Floor((_backPlayer.GetTime() + TIME_OFFSET) * FRAMERATE);
            if (index != _lastReadIndex)
            {
                _lastReadIndex = index;
                var source = BackScreen.material.mainTexture;
                var material = BackScreen.material;
                foreach (var board in _boards)
                {
                    board.Blit(source, index, material);
                }
            }
        }
    }
}
