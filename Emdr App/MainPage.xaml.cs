#define DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Syncfusion.Buttons.XForms;
using Syncfusion.Buttons;
using System.Collections.ObjectModel;
using Plugin.SimpleAudioPlayer;
using System.Collections;

#if DEBUG
using System.Diagnostics;
#endif

namespace Emdr_App
{

    public partial class MainPage : ContentPage
    {
        EmdrMode mode = EmdrMode.OptionsAndStaticBall;
        EmdrModel emdrModel = new EmdrModel
        {
            Size = (int)Math.Round(0.5 * 100),
            Color = Color.Yellow,
            Brightness = 30,
            Speed = 10,
            Direction = MoveDirection.Right,
            Platform = TargetPlatform.Software,
            UseLargeTappers = false,
            UseSmallTappers = false,
            UseLight = true,
            UseSound = true
        };

        bool centerBall = true;
        /// <summary>
        /// delay in miliseconds when ball is moving
        /// </summary>
        const int emdrStepInterval = 15;

        int emdrStepRealInterval = emdrStepInterval;
        DateTime lastEmdrStepStarted = DateTime.MinValue;
        DateTime currentEmdrStepStarted = DateTime.MinValue;

        // sound properties
        ISimpleAudioPlayer soundPlayer = CrossSimpleAudioPlayer.Current; // CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();

        bool BLEScanCompleteFlag = false;

        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Color.Green.ToSKColor(),
            StrokeWidth = 25
        };

        SKPaint blackPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Color.Black.ToSKColor(),
            StrokeWidth = 25
        };

        
        SKSurface surface = null;
        SKCanvas canvas = null;
        public MainPage()
        {
            InitializeComponent();

            ArduinoHTTPUtils.IP = arduinoIPAddressEntry.Text;

            if (emdrModel.Platform == TargetPlatform.Software)
                thisDeviceButton.IsChecked = true;
            else
                arduinoButton.IsChecked = true;
            
            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            Utils.LoadPlayer(soundPlayer);

            
            Device.StartTimer(TimeSpan.FromMilliseconds(emdrStepInterval), EmdrStep);
        }


        public bool EmdrStep()
        {
            if(lastEmdrStepStarted == DateTime.MinValue)
            {
                lastEmdrStepStarted = DateTime.Now.AddMilliseconds(0-emdrStepInterval);
            }
            currentEmdrStepStarted = DateTime.Now;
            
            if (mode == EmdrMode.MovingBall || mode == EmdrMode.OptionsAndMovingBall)
            {
                if (emdrModel.Direction == MoveDirection.Right && emdrModel.X + emdrModel.Size > canvasView.CanvasSize.Width)
                {
                    emdrModel.Direction = MoveDirection.Left;

                }
                else if (emdrModel.Direction == MoveDirection.Left && emdrModel.X - emdrModel.Size < 0)
                {
                    emdrModel.Direction = MoveDirection.Right;

                }

                // play sound (only if it is selected in options
                if (emdrModel.UseSound && emdrModel.Platform == TargetPlatform.Software)
                {
                    // fix a bug for duration reset to 0 on Android
                    if (soundPlayer.Duration == 0)
                        Utils.LoadPlayer(soundPlayer);

                    // if ball is in the right part of the screen - play rightSoundPlayer, else play left
                    if (emdrModel.X < canvasView.CanvasSize.Width / 2 && soundPlayer.Balance <= 0)
                    {
                        if (soundPlayer.IsPlaying)
                            soundPlayer.Stop();

                        soundPlayer.Balance = 1;
                        soundPlayer.Play();
                    }
                    else if (emdrModel.X > canvasView.CanvasSize.Width / 2 && soundPlayer.Balance >= 0)
                    {
                        if (soundPlayer.IsPlaying)
                            soundPlayer.Stop();

                        soundPlayer.Balance = -1;
                        soundPlayer.Play();
                    }
                    else
                    {
                        if (!soundPlayer.IsPlaying)
                            soundPlayer.Play();
                    }
                }
                
                TimeSpan time = currentEmdrStepStarted.Subtract(lastEmdrStepStarted);
                emdrStepRealInterval = (int)Math.Round(time.TotalMilliseconds);
                lastEmdrStepStarted = currentEmdrStepStarted;
                // area between ball touching both sides of canvas view / stepInterval * speed/10 (because we keep speed times 10 to be int and the same for arduino and PC)
                int steps = (int)Math.Round((canvasView.Width - 2 * emdrModel.Size) / emdrStepInterval * ((double)emdrModel.Speed / 10));
                
                emdrModel.Move(steps);
                if (emdrModel.UseLight && emdrModel.Platform == TargetPlatform.Software)
                {
                    canvasView.InvalidateSurface();
                }
            }
            else
            {
                // static ball, no sound
                soundPlayer.Stop();
            }

            return true;
        }
        public void SetEmdrMode(EmdrMode newMode)
        {
            if (mode == newMode)
            {
                return;
            }
            mode = newMode;

            switch (mode)
            {
                case EmdrMode.StaticBall:
                    {
                        optionsFrame.IsVisible = false;
                        break;
                    }
                case EmdrMode.MovingBall:
                    {
                        optionsFrame.IsVisible = false;
                        break;
                    }
                case EmdrMode.OptionsAndMovingBall:
                    {
                        optionsFrame.IsVisible = true;
                        previewGrid.IsVisible = true;
                        initGrid.IsVisible = false;
                    //    this.arduinoSettingsPanel.IsVisible = targetPlatformPanel.IsVisible = typesOfStimulationPanel.IsVisible = false;
                    //    this.speedPanel.IsVisible = this.sizePanel.IsVisible = this.colorPanel.IsVisible = true;
                        emdrModel.Direction = MoveDirection.Right;
                        break;
                    }
                case EmdrMode.OptionsAndStaticBall:
                    {
                        optionsFrame.IsVisible = true;
                        previewGrid.IsVisible = false;
                        initGrid.IsVisible = true;
                        //                        targetPlatformPanel.IsVisible = typesOfStimulationPanel.IsVisible = true;
                        //                        this.arduinoSettingsPanel.IsVisible = emdrModel.Platform == TargetPlatform.Arduino;
                        //                        this.brightnessPanel.IsVisible = this.speedPanel.IsVisible = this.sizePanel.IsVisible = this.colorPanel.IsVisible = false;

                        break;
                    }

            }
            centerBall = true;
        }

        private void canvasView_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            surface = e.Surface;
            canvas = surface.Canvas;

            if (centerBall)
            {
                emdrModel.X = e.Info.Width / 2;
                emdrModel.Y = e.Info.Height / 2;
                centerBall = false;

            }

            canvas.Clear(SKColors.Black);

            // draw only if light should be used
            if(emdrModel.UseLight)
                canvas.DrawCircle(emdrModel.X, emdrModel.Y, emdrModel.Size, paint);
        }



        private void canvasView_Touch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Released:
                    {
                        emdrModel.X = (int)Math.Round(canvasView.X + canvasView.Width / 2);
                        emdrModel.Y = (int)Math.Round(canvasView.Y + canvasView.Height / 2);
                        EmdrMode newMode = mode;
                        switch (mode){
                            case EmdrMode.OptionsAndStaticBall:
                                {
                                    newMode = EmdrMode.OptionsAndMovingBall;
                                    break;
                                }
                            case EmdrMode.OptionsAndMovingBall:
                                {
                                    newMode = EmdrMode.MovingBall;
                                    break;
                                }
                            case EmdrMode.MovingBall:
                                {
                                    newMode = EmdrMode.StaticBall;
                                    break;
                                }
                            case EmdrMode.StaticBall:
                                {
                                    newMode = EmdrMode.OptionsAndMovingBall;
                                    break;
                                }
                        }
                        
                        SetEmdrMode(newMode);

                        // update the UI
                        canvasView.InvalidateSurface();
                        break;
                    }
            }
            e.Handled = true;


        }

        private void chipscolorpicker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            if (e.PropertyName == "SelectedColor")
            {
                emdrModel.Color = ((ColorPicker)chipscolorpicker).SelectedColor;
                paint.Color = Extensions.ToSKColor(emdrModel.Color);
                canvasView.InvalidateSurface();

                // send data to Arduino
                if (emdrModel.Platform == TargetPlatform.Arduino)
                    ArduinoHTTPUtils.SendStart(emdrModel);
            }

        }

        private void sizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            emdrModel.Size = (int)Math.Round(e.NewValue * 100);
            canvasView.InvalidateSurface();
        }

        private void speedSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            emdrModel.Speed = (int)Math.Round(e.NewValue);
            // send data to Arduino
            if (emdrModel.Platform == TargetPlatform.Arduino)
                ArduinoHTTPUtils.SendStart(emdrModel);
        }

        private void canvasView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width" || e.PropertyName == "CanvasSize" || e.PropertyName == "Height") {
                centerBall = true;
                canvasView.InvalidateSurface();
            }
        }

        private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            infoLabel.Text = thisDeviceButton.IsChecked ?
                "Select what device and type of stimulation. Click Preview." :
                "Click Scan to see all available devices. Select your device, click Connect. Select stimulation. Click Preview.";
            tappersPanel.IsVisible = BLEPanel.IsVisible = this.arduinoSettingsPanel.IsVisible = arduinoButton.IsChecked;
            emdrModel.Platform = thisDeviceButton.IsChecked ? TargetPlatform.Software : TargetPlatform.Arduino;
        }


        private void previewButton_Clicked(object sender, EventArgs e)
        {
            soundCheckBox.IsChecked = emdrModel.UseSound;
            lightCheckBox.IsChecked = emdrModel.UseLight;
            tappersSmallCheckBox.IsChecked = emdrModel.UseSmallTappers;
            tappersLargeCheckBox.IsChecked = emdrModel.UseLargeTappers;

            speedSlider.Value = emdrModel.Speed;
            chipscolorpicker.SelectedColor = emdrModel.Color;
            sizeSlider.Value = emdrModel.Size;
            brightnessSlider.Value = emdrModel.Brightness;

            previewGrid.IsVisible = true;
            initGrid.IsVisible = false;

            if (mode == EmdrMode.OptionsAndStaticBall)
            {
                SetEmdrMode(EmdrMode.OptionsAndMovingBall);
                colorPanel.IsVisible = emdrModel.UseLight;
                sizePanel.IsVisible = emdrModel.Platform == TargetPlatform.Software && emdrModel.UseLight;
                brightnessPanel.IsVisible = emdrModel.Platform == TargetPlatform.Arduino && emdrModel.UseLight;

                // send data to Arduino
                if (emdrModel.Platform == TargetPlatform.Arduino)
                {
                    ArduinoHTTPUtils.SendStart(emdrModel);

                }
            }
        }


        private void brightnessSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            emdrModel.Brightness = (int)Math.Round(e.NewValue);
            // send data to Arduino
            if (emdrModel.Platform == TargetPlatform.Arduino)
                ArduinoHTTPUtils.SendStart(emdrModel);

        }

        private void arduinoLEDFromEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string value = arduinoLEDFromEntry.Text;
            string newValue = MakeNumeric(value);
            if (!newValue.Equals(value))
            {
                arduinoLEDFromEntry.Text = newValue;
            }
        }

        private void arduinoLEDToEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string value = arduinoLEDToEntry.Text;
            string newValue = MakeNumeric(value);
            if (!newValue.Equals(value))
            {
                arduinoLEDToEntry.Text = newValue;
            }
        }

        private string MakeNumeric(string value)
        {
            
            string newValue = "";
            if (value == null)
            {
                return newValue;
            }
            foreach (char c in value)
            {
                // Check for numeric characters (0-9)
                if ((c >= '0' && c <= '9'))
                {
                    newValue = string.Concat(newValue, c);
                }
                else
                {
                    break;
                }
            }
            return newValue;
        }

       
       
        private void backButton_Clicked(object sender, EventArgs e)
        {
            previewGrid.IsVisible = false;
            initGrid.IsVisible = true;
            if (emdrModel.Platform == TargetPlatform.Arduino)
                ArduinoHTTPUtils.SendStop();

            SetEmdrMode(EmdrMode.OptionsAndStaticBall);
        }

        private void setLEDRange_Clicked(object sender, EventArgs e)
        {
            // send data to Arduino
            if (emdrModel.Platform == TargetPlatform.Arduino)
                ArduinoHTTPUtils.SendStart(emdrModel, this.arduinoLEDFromEntry.Text, this.arduinoLEDToEntry.Text);

        }

        private void lightCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            emdrModel.UseLight = lightCheckBox.IsChecked;
        }

        private void soundCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            emdrModel.UseSound = soundCheckBox.IsChecked;
        }

        private void tappersSmallCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            emdrModel.UseSmallTappers = tappersSmallCheckBox.IsChecked;
        }

        private void tappersLargeCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            emdrModel.UseLargeTappers = tappersLargeCheckBox.IsChecked;
        }

        private void arduinoIPAddressEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ArduinoHTTPUtils.IP = arduinoIPAddressEntry.Text;
        }
    }
}
