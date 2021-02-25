using AgoraUWP;
using AgoraWinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ScreenCaptureDemo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AgoraUWP.AgoraRtc engine;

        public string VendorKey { get; set; } = "c021e195268048418d8176e3d7a8e8bd";
        public string ChannelToken { get; set; } = "006c021e195268048418d8176e3d7a8e8bdIAAiT5AvKWCxEOnFqT5byXMnAX84QjfAZx5pJdgqh4I5ydxEAZkAAAAAEABwjq6P46ckYAEAAQDipyRg";
        public string ChannelName { get; set; } = "666";
        public ulong UID { get; set; } = 0;

        public MainPage()
        {
            this.InitializeComponent();
            AgoraUWP.AgoraRtc.RequestCameraAccess();

            btnScreenCapture.Click += StartScreenCapture;
            btnMirrorLocalVideo.Checked += ChangeRenderMode;
            btnMirrorLocalVideo.Unchecked += ChangeRenderMode;
            cbLocalVideoRenderMode.SelectionChanged += ChangeRenderMode;

        }

        private void ChangeRenderMode(object sender, RoutedEventArgs e)
        {
            var mirrorMode = btnMirrorLocalVideo.IsChecked.GetValueOrDefault(false) ? VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED : VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_DISABLED;
            var renderMode = RENDER_MODE_TYPE.RENDER_MODE_ADAPTIVE;
            switch (cbLocalVideoRenderMode.SelectedIndex)
            {
                case 0: renderMode = RENDER_MODE_TYPE.RENDER_MODE_FIT; break;
                case 1: renderMode = RENDER_MODE_TYPE.RENDER_MODE_HIDDEN; break;
                case 2: renderMode = RENDER_MODE_TYPE.RENDER_MODE_FILL; break;
            }
            this.engine?.SetLocalScreenVideoRenderMode(renderMode, mirrorMode);
        }

        private async void StartScreenCapture(object sender, RoutedEventArgs e)
        {
            if (this.engine != null)
            {
                this.engine.LeaveChannel();
                this.engine.Dispose();
                this.engine = null;
            }

            this.engine = new AgoraUWP.AgoraRtc(this.VendorKey);
            log("Set Video Encoder Configuration",
                engine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
                {
                    dimensions = new VideoDimensions { width = 1920, height = 1080 },
                    frameRate = FRAME_RATE.FRAME_RATE_FPS_60,
                    minFrameRate = -1,
                    bitrate = 1600,
                    minBitrate = 0,
                    orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
                    degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_QUALITY,
                    mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_DISABLED,
                }));
            this.log("enable video", this.engine.EnableVideo());
            this.log("disable audio", this.engine.DisableAudio());
            this.log("disable lastmile test", this.engine.DisableLastmileTest());
            this.log("set channel profile", this.engine.SetChannelProfile(AgoraWinRT.CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING));
            this.log("set client role", this.engine.SetClientRole(AgoraWinRT.CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER));
            this.log("mute all remote audio", this.engine.MuteAllRemoteAudioStreams(true));
            this.log("mute all remote video", this.engine.MuteAllRemoteVideoStream(true));
            this.log("join channel", this.engine.JoinChannel(ChannelToken, ChannelName, "", UID));

            this.engine.SetupLocalScreenVideo(new SpriteVisualVideoCanvas
            {
                Target = screenVideo,
                RenderMode = RENDER_MODE_TYPE.RENDER_MODE_FIT,
                MirrorMode = btnMirrorLocalVideo.IsChecked.GetValueOrDefault(false) ? VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED : VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_DISABLED
            });
            await this.engine.StartScreenCapture();
        }

        private void log(String operation, long result)
        {
            _ = txtResult.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    txtResult.Text += String.Format("agora {0} result is {1}\n", operation, result);
                });
        }

    }
}
