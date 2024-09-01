
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Video.Components.AVPro;
using System;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Data;

namespace Chikuwa.Imgin
{
    public class ImginLoader : UdonSharpBehaviour
    {
        readonly int MAX_SCREEN_SIZE = 1280;
        readonly float READ_TIME_OFFSET = 0.05f;
        readonly float FRAMERATE = 4f;
        readonly int PADDING_FRAMES = 4;
        public MeshRenderer BackScreen;
        public VRCUrl VideoURL;
        public VRCUrl JSONURL;

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
            VRCStringDownloader.LoadUrl(JSONURL, (IUdonEventReceiver)this);
        }

        public void AddBoard(ImginBoard board)
        {
            _boards = ArrayUtils.Append(_boards, board);
        }

        public override void OnVideoEnd()
        {
            gameObject.SetActive(false);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            VRCJson.TryDeserializeFromJson(result.Result, out DataToken images);
            if (images.TokenType != TokenType.DataList)
            {
                Debug.LogError("Invalid JSON format.");
                return;
            }
            float[] ratios = new float[images.DataList.Count];
            for (int i = 0; i < images.DataList.Count; i++)
            {
                var image = images.DataList[i].DataDictionary;
                ratios[i] = (float)(image["height"].Double / image["width"].Double);
            }
            foreach (var board in _boards)
            {
                board.ApplyAspectRatios(ratios);
            }
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError($"Error loading string: {result.ErrorCode} - {result.Error}");
        }

        void LateUpdate()
        {
            if (!_backPlayer.IsPlaying)
            {
                return;
            }

            var index = (int)Math.Floor((_backPlayer.GetTime() - READ_TIME_OFFSET) * FRAMERATE) - PADDING_FRAMES;
            if (index >= 0 && _lastReadIndex != index)
            {
                _lastReadIndex = index;
                var material = BackScreen.material;
                foreach (var board in _boards)
                {
                    board.ApplyImage(material, index);
                }
            }
        }
    }
}
