namespace SkyWatcherMotorMoveApp
{

	public class SkyWatcherSimpleController
	{
		const int AXISID_RA_Az = 1;
		const int AXISID_Dec_Alt = 2;

		Mount mount;
		double currentSpeed; //for button
		double currentSpeedX; //for joy stick
		double currentSpeedY; //for joy stick

		public SkyWatcherSimpleController()
		{
			mount = new Mount();
		}
		public void setSpeed(double speed)
		{
			currentSpeed = speed;
		}
		public void ButtonPushed_UP()
		{
			mount.changeSpeed(AXISID_Dec_Alt, -currentSpeed);
		}
		public void ButtonPushed_DOWN()
		{
			mount.changeSpeed(AXISID_Dec_Alt, currentSpeed);
		}
		public void ButtonPushed_RIGHT()
		{
			mount.changeSpeed(AXISID_RA_Az, -currentSpeed);
		}
		public void ButtonPushed_LEFT()
		{
			mount.changeSpeed(AXISID_RA_Az, currentSpeed);
		}
		public void ButtonRELEASED()
		{
			mount.allStop();
		}

		//for joy stick type
		public void setSpeedX(double speedX)
		{
			if (speedX != currentSpeedX)
			{
				currentSpeedX = speedX;
				mount.changeSpeed(AXISID_RA_Az, speedX);
			}
		}
		public void setSpeedY(double speedY)
		{
			if (speedY != currentSpeedY)
			{
				currentSpeedY = speedY;
				mount.changeSpeed(AXISID_Dec_Alt, speedY);
			}
		}

		public string getLocationX()
		{
			return mount.getLocation(AXISID_RA_Az);
		}

		public string getLocationY()
		{
			return mount.getLocation(AXISID_Dec_Alt);
		}

		public bool isConnected()
		{
			if (mount == null) return false;
			return mount.isConnected();
		}

	}

}