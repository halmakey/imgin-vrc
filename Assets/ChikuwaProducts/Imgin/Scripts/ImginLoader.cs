
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Video.Components.AVPro;
using System;
using VRC.SDK3.StringLoading;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.Data;
using VRC.SDK3.Video.Components;

namespace Chikuwa.Imgin
{
    public class ImginLoader : UdonSharpBehaviour
    {
        readonly float FRAMERATE = 2f;
        readonly int PADDING_FRAMES = 4;
        readonly float READ_OFFSET = 0.05f;

        [SerializeField]
        private VRCUrl _videoURL;
        [SerializeField]
        public VRCUrl _jsonURL;
        [SerializeField]
        private float _delaySeconds;
        private Renderer _backScreenRenderer;
        private MaterialPropertyBlock _backScreenPropertyBlock;

        private VRCUnityVideoPlayer _backPlayer;
        private ImginBoard[] _boards = Array.Empty<ImginBoard>();
        private int _lastReadIndex;
        private DataList _jsonData;

        void Start()
        {
            _backPlayer = GetComponent<VRCUnityVideoPlayer>();
            _backPlayer.Stop();
            _backPlayer.EnableAutomaticResync = false;
            _backPlayer.Loop = false;
            
            _backScreenRenderer = GetComponent<MeshRenderer>();
            _backScreenPropertyBlock = new MaterialPropertyBlock();

            SendCustomEventDelayedSeconds("StartLoad", _delaySeconds);
        }

        public void StartLoad()
        {
            _lastReadIndex = -1;
            _backPlayer.LoadURL(_videoURL);
            VRCStringDownloader.LoadUrl(_jsonURL, (IUdonEventReceiver)this);
        }

        public void AddBoard(ImginBoard board)
        {
            var prev = _boards;
            _boards = new ImginBoard[prev.Length + 1];
            Array.Copy(prev, _boards, prev.Length);
            _boards[prev.Length] = board;
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

            _jsonData = images.DataList;

            PlayIfReady();
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError($"Error loading string: {result.ErrorCode} - {result.Error}");
        }

        public override void OnVideoReady()
        {
            PlayIfReady();
        }

        void PlayIfReady()
        {
            if (_jsonData == null || !_backPlayer.IsReady)
            {
                return;
            }

            _backPlayer.Play();
        }

        void LateUpdate()
        {
            if (!_backPlayer.IsPlaying)
            {
                return;
            }

            var index = (int)Math.Floor(_backPlayer.GetTime() * FRAMERATE) - PADDING_FRAMES;
            if (index >= 0 && _lastReadIndex != index && index < _jsonData.Count)
            {
                _lastReadIndex = index;

                SendCustomEventDelayedSeconds("ReadFrame", READ_OFFSET, EventTiming.LateUpdate);
            }
        }

        public void ReadFrame()
        {
            var index = _lastReadIndex;
            var item = _jsonData[index].DataDictionary;
            var ratio = (float)(item["height"].Double / item["width"].Double);

            _backScreenRenderer.GetPropertyBlock(_backScreenPropertyBlock);

            var texture = _backScreenPropertyBlock.GetTexture("_MainTex");

            foreach (var board in _boards)
            {
                board.ApplyImage(texture, index, ratio);
            }
        }
    }
}
