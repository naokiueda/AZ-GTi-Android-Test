using System;

namespace SkyWatcherMotorMoveApp
{
	public class Axis
	{


		int AxisId;
		int TMR_Freq;//16000000 if AZ GTi
		int CPR;// 2073600 for AZ GTi


		private UdpPort port;
		private double currentSpeed;
		private int isRunning;
		private int isTracking;
		private int isCCW;
		private int isFastMode;
		private int TwoAxesMustStartSeparetely;
		private bool isConnected = false;

		public Axis(int axisId, UdpPort _udpPort)
		{
			//Init
			AxisId = axisId;
			port = _udpPort;
			currentSpeed = 0;
			isRunning = 0;
			isTracking = 0;
			isCCW = 0;
			isFastMode = 0;

			string msg = "";
			//Get ExtendedInquire
			msg = ":q3010000";
			string response = port.sendRecvMsg(msg);
			if (response == "")
			{
				isConnected = false;
			}
			else if (response[0] == '=')
			{
				isConnected = true;
				TwoAxesMustStartSeparetely = response[3] & 0x02;//B1: Tow axes must start seperately
			}


			//Get CPR
			msg = ":a" + AxisId.ToString();
			CPR = convertProtcolHexToInt(port.sendRecvMsg(msg));

			//Get TMR_Freq
			msg = ":b" + AxisId.ToString();
			TMR_Freq = convertProtcolHexToInt(port.sendRecvMsg(msg));


		}

		private int convertProtcolHexToInt(string swValue)
		{
			if (swValue[0] != '=') return 0;

			string hex = "";
			for (int i = 1; i < swValue.Length; i += 2)
			{
				hex = swValue.Substring(i, 2) + hex;
			}
			int v = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
			return v;

		}
		public bool SetSpeed(double siderealSpeedRatio)
		{
			currentSpeed = siderealSpeedRatio;
			return true;
		}
		public double GetSpeed()
		{
			return currentSpeed;
		}

		public bool SetTheMotionMode()
		{
			string msg = ":G" + AxisId.ToString() + ((Math.Abs(currentSpeed) > 128) ? 3 : 1).ToString() + (currentSpeed < 0 ? 1 : 0).ToString();// when high speed slewing is required(For example, move an axis with higher then 128x sidereal rate).
			string response = port.sendRecvMsg(msg);
			return (response[0] == '=');
		}

		public bool SetT1parameter()
		{
			double Speed_DegPerSec = (double)(360) / (double)((23 * 3600 + 56 * 60 + 4)); // 360/(23:56:04 in sec) = 0.004178079 deg/sec =恒星時

			double t1_test = Math.Abs(currentSpeed) * (double)TMR_Freq * 360.0 / Speed_DegPerSec / (double)CPR;

			Speed_DegPerSec *= Math.Abs(currentSpeed); //For desired speed
			double T1_Preset_d = (double)TMR_Freq * 360.0 / Speed_DegPerSec / (double)CPR;//T1_Preset = TMR_Freq * 360 / Speed_DegPerSec / CPR


			int T1_Preset = (int)Math.Floor(T1_Preset_d + 0.5);//Round 
			int d2 = T1_Preset / 65536;
			int d1 = (T1_Preset - d2 * 65536) / 256;
			int d0 = T1_Preset - d2 * 65536 - d1 * 256;

			string msg = ":I" + AxisId.ToString() + d0.ToString("X2") + d1.ToString("X2") + d2.ToString("X2");

			string response = port.sendRecvMsg(msg);
			return (response[0] == '=');
		}

		public bool Start()
		{
			string msg = ":J" + AxisId.ToString();
			string response = port.sendRecvMsg(msg);
			return (response[0] == '=');
		}

		public bool Stop()
		{
			currentSpeed = 0;
			string msg = ":K" + AxisId.ToString();
			string response = port.sendRecvMsg(msg);
			return (response[0] == '=');
		}

		public void GetStatus()
		{
			string msg = ":f" + AxisId.ToString();
			string status = port.sendRecvMsg(msg);

			isRunning = (status[2] == '0' ? 0 : 1);
			isTracking = status[1] & 0x01;
			isCCW = status[1] & 0x02;
			isFastMode = status[1] & 0x04;

		}

		public string GetPosition()
		{
			string msg = ":j" + AxisId.ToString();
			string status = port.sendRecvMsg(msg);

			if (status[0] == '=')
			{
				int pos = convertProtcolHexToInt(status);
				pos -= 0x00800000;
				return pos.ToString();
			}
			else
			{
				return status;
			}
		}

		public bool IsRunning()
		{
			return isRunning == 1;
		}

		public bool IsTracking()
		{
			return isTracking == 1;
		}

		public bool IsCCW()
		{
			return isCCW == 1;
		}

		bool IsFastMode()
		{
			return isFastMode == 1;
		}

		public bool doesTwoAxesMustStartSeparetely()
		{
			return (TwoAxesMustStartSeparetely == 0x00);
		}
		public bool IsConnected()
		{
			return isConnected;
		}
		public int changeSpeed(double speed)
		{
			//Check current status

			//If speed if same, do nothing
			if (GetSpeed() == speed)
			{
				return 1;
			}

			//If 0, Stop
			if (speed == 0)
			{
				Stop();
				return 1;
			}
			GetStatus();

			//If motor must move separately, wait for another axis stop
			//if (doesTwoAxesMustStartSeparetely())
			//{
			//	while (anotherAxis->IsRunning())
			//	{
			//		System.Threading.Thread.Sleep(100);
			//		anotherAxis->GetStatus();
			//	}
			//}

			//If stopped now, just run
			if (!IsRunning())
			{
				SetSpeed(speed);
				SetTheMotionMode();
				SetT1parameter();
				Start();
				return 1;
			}

			//If in Goto Mode, Stop
			if (!IsTracking())
			{
				Stop();
				while (IsRunning())
				{
					System.Threading.Thread.Sleep(100);      //Wait unti stop
					GetStatus();
				}
				SetSpeed(speed);
				SetTheMotionMode();
				SetT1parameter();
				Start();
				return 1;
			}

			//If running in fast mode nad try to change speed, stop once
			if (IsRunning()
				&& IsFastMode()
				&& GetSpeed() != speed //Speed change
				)
			{
				Stop();
				while (IsRunning())
				{
					System.Threading.Thread.Sleep(100);      //Wait unti stop
					GetStatus();
				}
				SetSpeed(speed);
				SetTheMotionMode();
				SetT1parameter();
				Start();
				return 1;
			}

			//if rotation direction change, stop once
			if (GetSpeed() * speed < 0)
			{
				Stop();
				while (IsRunning())
				{
					System.Threading.Thread.Sleep(100);      //Wait unti stop
					GetStatus();
				}
				SetSpeed(speed);
				SetTheMotionMode();
				SetT1parameter();
				Start();
				return 1;
			}

			//while running in Slow mode and same direction
			Stop();
			while (IsRunning())
			{
				System.Threading.Thread.Sleep(100);      //Wait unti stop
				GetStatus();
			}
			SetSpeed(speed);
			SetTheMotionMode();
			SetT1parameter();
			Start();
			return 1;
		}


	}
}