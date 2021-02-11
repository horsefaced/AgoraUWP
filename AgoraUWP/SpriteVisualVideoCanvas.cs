using AgoraWinRT;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Media.Capture.Frames;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;

namespace AgoraUWP
{
    public class SpriteVisualVideoCanvas : VideoCanvas
    {
        private const int DEFAULT_WIDTH = 400;
        private const int DEFAULT_HEIGHT = 400;

        private CanvasDevice canvasDevice;
        private Compositor compositor;
        private CompositionDrawingSurface surface;
        private SpriteVisual visual;
        private UIElement target;

        public SpriteVisualVideoCanvas()
        {
            this.canvasDevice = new CanvasDevice();
            this.compositor = Window.Current.Compositor;
            var compositionGraphicDevice = CanvasComposition.CreateCompositionGraphicsDevice(this.compositor, canvasDevice);
            this.surface = compositionGraphicDevice.CreateDrawingSurface(new Size(DEFAULT_WIDTH, DEFAULT_HEIGHT),
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            this.visual = this.compositor.CreateSpriteVisual();
            this.visual.RelativeSizeAdjustment = Vector2.One;
            this.visual.Brush = this.compositor.CreateSurfaceBrush(this.surface);
        }

        public override VIDEO_MIRROR_MODE_TYPE MirrorMode
        {
            get => base.MirrorMode;
            set
            {
                base.MirrorMode = value;
                if (value == VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_ENABLED)
                {
                    this.visual.RotationAxis = new Vector3(0, 1, 0);
                    this.visual.RelativeOffsetAdjustment = new Vector3(1, 0, 0);
                    this.visual.RotationAngleInDegrees = 180;
                }
                else
                {
                    this.visual.RotationAxis = new Vector3(0, 1, 0);
                    this.visual.RelativeOffsetAdjustment = new Vector3(0, 0, 0);
                    this.visual.RotationAngleInDegrees = 0;
                }
            }
        }

        public override RENDER_MODE_TYPE RenderMode
        {
            get => base.RenderMode;
            set
            {
                base.RenderMode = value;
            }
        }

        public override object Target
        {
            get => this.target;
            set
            {
                this.target = value as UIElement;
                if (this.target != null)
                {
                    ElementCompositionPreview.SetElementChildVisual(this.target, this.visual);
                }
            }
        }

        public override void Render(VideoFrame frame)
        {
            if (this.target == null) return;
            var nvBuffer = Utils.ConvertToNv12(frame);
            using (var image = Utils.ConvertToImage(nvBuffer, (int)frame.width, (int)frame.height))
            using (var bitmap = CanvasBitmap.CreateFromSoftwareBitmap(this.canvasDevice, image))
            using (var session = CanvasComposition.CreateDrawingSession(this.surface))
            {
                session.Clear(Colors.Transparent);
                session.DrawImage(bitmap);
            }
        }

        public override void Render(MediaFrameReference frame)
        {
            if (this.target == null) return;

            using (var softwareBitmap = Utils.ConvertToImage(frame?.VideoMediaFrame))
            using (var bitmap = CanvasBitmap.CreateFromSoftwareBitmap(this.canvasDevice, softwareBitmap))
            using (var session = CanvasComposition.CreateDrawingSession(this.surface))
            {
                session.Clear(Colors.Transparent);
                session.DrawImage(bitmap);
            }
        }
    }
}
