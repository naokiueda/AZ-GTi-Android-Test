namespace SkyWatcherMotorMoveApp
{
	public class Mount
	{

		private Axis axis1;
		private Axis axis2;
		UdpPort port;

		public Mount()
		{
			port = new UdpPort();
			axis1 = new Axis(1, port);
			axis2 = new Axis(2, port);
		}
		public bool isConnected()
		{
			return axis1.IsConnected() && axis2.IsConnected();
			//if(axis1.is)
			//bool connected = false;
			//try
			//{
			//	string response = port.sendRecvMsg(":e1");
			//	if (response.StartsWith("="))
			//	{
			//		connected = true;
			//	}

			//}
			//catch (System.Net.Sockets.SocketException se)
			//{
			//	connected = false;
			//}
			//return connected;
		}
		public int changeSpeed(int axisId, double speed)
		{
			return (axisId == 1) ? axis1.changeSpeed(speed) : axis2.changeSpeed(speed);
		}
		public string getLocation(int axisId)
		{
			return (axisId == 1) ? axis1.GetPosition() : axis2.GetPosition();

		}
		public void allStop()
		{
			axis1.Stop();
			axis2.Stop();
		}

		~Mount()
		{

		}
	}
}