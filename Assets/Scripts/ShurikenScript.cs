using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static Gabsee.NetManager;

namespace Gabsee
{
	public class ShurikenScript : MonoBehaviour
	{

		private float rotationVelocity = 20.0f;
		private Vector3 rotationDirection = new Vector3(0, 0, 1);
		private bool rotationOn = false;
		private NetManager netManager = new NetManager();
		private float timeToStartSync = 5.0f;
		private float timeToNextSync = 0.0f;
		private float myLatency = -1.0f;

		// Use this for initialization
		void Start()
		{
			float now = Time.time;
			netManager.OnNetworkReceive = delegate (IPEndPoint endPoint, byte[] data)
			{
				// New peer connected. Rotation restart and starting again in data received time - half our latency, as we only
                // count half the way from a request, client-server-client, as we only receive from server to client part.
				transform.rotation = new Quaternion(0, 0, 0,0);
				float delayToStartSync = System.BitConverter.ToSingle(data, 0);
				timeToNextSync = delayToStartSync - myLatency/2;
			};
			netManager.OnConnection = delegate (IPEndPoint endPoint)
			{
				// Let's assume we have an ideal world here and we will receive our own connection message with no interferences,
				// so we have the client-server-client request latency. We may better keep the endpoint here and do a send to only
                // us, but just for not adding more complexity to OnNetworkReceive method, lets assume it'll work perfectly.
				if (myLatency < 0)
				{
					myLatency = Time.time - now;
				}
			};
			// We need here to connect to server, but not sure how is expected to be done.
			// Main idea is to connect and send a message indicating all peers to restart rotation in timeToStartSync seconds.
			// netManager.StartServer(8080);
			netManager.StartClient("localhost", 8080);
			// while (myLatency < 0) { } // Wait till connection ends
			netManager.Send(System.BitConverter.GetBytes(timeToStartSync)); // Assuming this is async call.
		}

		// Update is called once per frame
		void Update()
		{
			netManager.Update();
			if (rotationOn)
			{
				transform.RotateAround(transform.position, rotationDirection, Time.deltaTime * rotationVelocity);
			}
			if (timeToNextSync > 0)
            {
				timeToNextSync -= Time.deltaTime;
				if (timeToNextSync <= 0)
                {
					rotationOn = true;
				}
			}
		}
	}
}