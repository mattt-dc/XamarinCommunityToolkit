using System;
using System.Diagnostics;
using System.IO;
using AVFoundation;
using Foundation;
using Photos;
using UIKit;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRenderer))]

namespace Xamarin.CommunityToolkit.UI.Views
{
	public class CameraViewRenderer : ViewRenderer<CameraView, FormsCameraView>
	{
		bool disposed;

		protected override void OnElementChanged(ElementChangedEventArgs<CameraView> e)
		{
			base.OnElementChanged(e);

			if (Control == null && !disposed)
			{
				SetNativeControl(new FormsCameraView());

				_ = Control ?? throw new NullReferenceException($"{nameof(Control)} cannot be null");
				Control.Busy += OnBusy;
				Control.Available += OnAvailability;
				Control.FinishCapture += FinishCapture;

				Control.SetBounds(Element.WidthRequest, Element.HeightRequest);
				Control.VideoStabilization = Element.VideoStabilization;
				Control.Zoom = (float)Element.Zoom;
				Control.RetrieveCameraDevice(Element.CameraOptions);
				Control.SwitchFlash(Element.FlashMode);
			}

			if (e.OldElement != null)
			{
				e.OldElement.ShutterClicked -= HandleShutter;
			}

			if (e.NewElement != null)
			{
				e.NewElement.ShutterClicked += HandleShutter;
			}
		}

		void OnBusy(object? sender, bool busy) => Element.IsBusy = busy;

		void OnAvailability(object? sender, bool available)
		{
			Element.MaxZoom = Control.MaxZoom;
			Element.IsAvailable = available;
		}

		void FinishCapture(object? sender, Tuple<NSObject?, NSError?> e)
		{
			if (Element == null || Control == null)
				return;

			if (e.Item2 != null)
			{
				Element.RaiseMediaCaptureFailed(e.Item2.LocalizedDescription);
				return;
			}

			var photoData = e.Item1 as NSData;

			// See TODO on CameraView.SavePhotoToFile
			// if (!Element.SavePhotoToFile && photoData != null)
			if (photoData != null)
			{
				var data = UIImage.LoadFromData(photoData)?.AsJPEG().ToArray();
				Device.BeginInvokeOnMainThread(() =>
				{
					Element.RaiseMediaCaptured(new MediaCapturedEventArgs(imageData: data));
				});
			}
			if (e.Item1 is NSUrl outputFileUrl)
			{
				Element.RaiseMediaCaptured(new MediaCapturedEventArgs(path: outputFileUrl.AbsoluteString));
			}
			Element.RaiseMediaCaptureFailed("failed");
			return;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;

			disposed = true;
			Control?.Dispose();
			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (Element == null || Control == null)
				return;

			switch (e.PropertyName)
			{
				case nameof(CameraView.CameraOptions):
					Control.RetrieveCameraDevice(Element.CameraOptions);
					break;
				case nameof(Element.Height):
				case nameof(Element.Width):
				case nameof(VisualElement.HeightProperty):
				case nameof(VisualElement.WidthProperty):
					Control.SetBounds(Element.Width, Element.Height);
					Control.SetOrientation();
					break;
				case nameof(CameraView.VideoStabilization):
					Control.VideoStabilization = Element.VideoStabilization;
					break;
				case nameof(CameraView.FlashMode):
					Control.SwitchFlash(Element.FlashMode);
					break;
				case nameof(CameraView.Zoom):
					Control.Zoom = (float)Element.Zoom;
					break;
			}
		}

		async void HandleShutter(object? sender, EventArgs e)
		{
			switch (Element.CaptureMode)
			{
				case CameraCaptureMode.Default:
				case CameraCaptureMode.Photo:
					if (Control != null)
						await Control.TakePhoto();
					break;
				case CameraCaptureMode.Video:
					if (Control == null)
						return;

					if (!Control.VideoRecorded)
						Control.StartRecord();
					else
						Control.StopRecord();
					break;
			}
		}
	}
}