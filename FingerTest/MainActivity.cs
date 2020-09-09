using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;

namespace SkyWatcherMotorMoveApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private TextView _vInfoArea;
        //private TextView _vInfoArea2;
        private static SkyWatcherSimpleController sc;
        private bool Initialized = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            _vInfoArea = FindViewById<TextView>(Resource.Id.info_area);
            //_vInfoArea2 = FindViewById<TextView>(Resource.Id.info_area2);
            _vInfoArea.Text = $"Accesing...\n";
            sc = new SkyWatcherSimpleController();
            if (!sc.isConnected())
            {
                _vInfoArea.Text = $"ERROR: Mount No Access\n";
            }
            else
            {
                Initialized = true;
                _vInfoArea.Text = $"RA/AZ  : STOP\nDEC/ALT: STOP\n";
            }
        }

        private float _fStartX, _fStartY;
        private int FastMode = 0;
        private double prevSpeedX = 0, prevSpeedY = 0;
        static double[] SyncScanAppStepSpeedFactor = { 0, 0.5, 1, 8, 16, 32, 64, 128, 400, 600, 800 };//speed 0 to 9 in SyncScanApp
        private DateTime prevChangeTimeX = DateTime.Now;
        private DateTime prevChangeTimeY = DateTime.Now;

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!Initialized)
            {
                _vInfoArea.Text = $"ERROR: Mount No Access\n";
                return false;
            }
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _fStartX = e.RawX;
                    _fStartY = e.RawY;
                    switch (e.PointerCount)
                    {
                        case 1:
                            FastMode = 0;
                            //txtInfo += "One finger";
                            break;
                        //case 2:
                        //    txtInfo += "Two fingers";
                        //    break;
                        default:
                            FastMode = 1;
                            //txtInfo += $"{e.PointerCount} fingers";
                            break;
                    }
                    prevChangeTimeX = DateTime.Now;
                    prevChangeTimeY = DateTime.Now;
                    break;
                case MotionEventActions.Move:
                    float dx = e.RawX - _fStartX;
                    float dy = e.RawY - _fStartY;

                    int rotX = dx < 0 ? -1 : 1, rotY = dy < 0 ? 1 : -1;
                    int stepX, stepY;

                    if (e.PointerCount > 1) FastMode = 1;

                    //300範囲くらいで
                    //if (FastMode == 0)
                    if (true)
                    {
                        // 25   75   125  175  225  275  325  
                        //__|____|____|____|____|____|____|_____________________
                        //NA  0    1    2    3    4    5    6
                        if (Math.Abs(dx) < 25) { stepX = 99; }
                        else
                        {
                            stepX = (int)(Math.Floor((Math.Abs(dx) - 25) / 50.0));
                            if (stepX > 6) stepX = 6;
                        }
                        if (Math.Abs(dy) < 25) { stepY = 99; }
                        else
                        {
                            stepY = (int)(Math.Floor((Math.Abs(dy) - 25) / 50.0));
                            if (stepY > 6) stepY = 6;
                        }
                        stepX *= rotX;
                        stepY *= rotY;
                    }
                    //else
                    //{
                    //    // 25   125  225    
                    //    //__|____|____|____
                    //    //NA  7    8    9  
                    //    if (Math.Abs(dx) < 25) { stepX = 99; }
                    //    else
                    //    {
                    //        stepX = (int)(Math.Floor((Math.Abs(dx) - 25) / 100.0+7));
                    //        if (stepX > 9) stepX = 9;
                    //    }
                    //    if (Math.Abs(dy) < 25) { stepY = 99; }
                    //    else
                    //    {
                    //        stepY = (int)(Math.Floor((Math.Abs(dy) - 25) / 100.0+7));
                    //        if (stepY > 9) stepY = 9;
                    //    }
                    //    stepX *= rotX;
                    //    stepY *= rotY;
                    //}

                    double DX = Math.Abs(stepX) == 99 ? 0 : (SyncScanAppStepSpeedFactor[Math.Abs(stepX) + 1] * (rotX < 0 ? -1 : 1));
                    double DY = Math.Abs(stepY) == 99 ? 0 : (SyncScanAppStepSpeedFactor[Math.Abs(stepY) + 1] * (rotY < 0 ? -1 : 1));
                    if (prevSpeedX != DX && (DateTime.Now - prevChangeTimeX).TotalMilliseconds > 200)
                    {
                        prevSpeedX = DX;
                        prevChangeTimeX = DateTime.Now;
                    }
                    if (prevSpeedY != DY && (DateTime.Now - prevChangeTimeY).TotalMilliseconds > 200)
                    {
                        prevSpeedY = DY;
                        prevChangeTimeY = DateTime.Now;
                    }

                    string txtInfo = $"Fast mode: {FastMode}\nStart Position: {_fStartX}, {_fStartY}\nPosition Delta: {dx}, {dy}\n\nAZ :{prevSpeedX}\nALT:{prevSpeedY}\n\n";
                    switch (e.PointerCount)
                    {
                        case 1:
                            txtInfo += "One finger";
                            break;
                        case 2:
                            txtInfo += "Two fingers";
                            break;
                        default:
                            txtInfo += $"{e.PointerCount} fingers";
                            break;
                    }

                    //_vInfoArea.Text = txtInfo;
                    if (DX == 0)
                    {
                        _vInfoArea.Text = $"RA/AZ  : STOP\n";
                        sc.setSpeedX(0);
                    }
                    else
                    {
                        _vInfoArea.Text = $"RA/AZ  : x{prevSpeedX}:{stepX}\n";
                        sc.setSpeedX(prevSpeedX);
                    }
                    //_vInfoArea.Text += "\n";
                    if (DY == 0)
                    {
                        _vInfoArea.Text += $"DEC/ALT: STOP\n";
                        sc.setSpeedX(0);
                    }
                    else
                    {
                        _vInfoArea.Text += $"DEC/ALT: x{prevSpeedY}:{stepY}\n";
                        sc.setSpeedY(prevSpeedY);
                    }



                    break;
                case MotionEventActions.Up:
                    //_vInfoArea.Text = string.Empty;
                    _vInfoArea.Text = $"RA/AZ  : STOP\nDEC/ALT: STOP\n";
                    sc.setSpeedX(0);
                    sc.setSpeedY(0);
                    break;
            }

            return base.OnTouchEvent(e);
        }
    }
}

