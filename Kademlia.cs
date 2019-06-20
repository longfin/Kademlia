using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	class Kademlia
	{
		static private List<NormalNode> _normalNodes;
		static private List<KademliaNode> _kademliaNodes;
		static private long tnormal, tkademlia;
		static private int utotalN, utotalK, dtotalN, dtotalK, muploadsN, muploadsK, mdownloadsN, mdownloadsK;
		static void Main(string[] args)
		{
			int NODECOUNT = 2 << 15;

			_normalNodes = new List<NormalNode>();
			_kademliaNodes = new List<KademliaNode>();

			for (int i = 0; i < NODECOUNT; i++)
			{
				CreateNormalNode(i);
				CreateKademliaNode(i);
			}

			SendOneBroadCast(NODECOUNT);
			//SendMultipleBroadCasts(NODECOUNT, 2000);
			//SendAllBroadCasts(NODECOUNT);

			if (AllKademliaNodeTouched(true))
				Console.WriteLine("All Kademlia node touched");

			if (AllNormalNodeTouched(true))
				Console.WriteLine("All Normal node touched");

			Console.WriteLine("=============================STATISTICS=============================");
			Console.WriteLine("\t\t\t\tNormal\t\tKademlia");
			Console.WriteLine("Elapsed Ticks Per Broadcast\t" + tnormal + "\t\t" + tkademlia);
			Console.WriteLine("Total Uploads\t\t\t" + utotalN + "\t\t" + utotalK);
			Console.WriteLine("Total Downloads\t\t\t" + dtotalN + "\t\t" + dtotalK);
			Console.WriteLine("Maximum Uploads\t\t\t" + muploadsN + "\t\t" + muploadsK);
			Console.WriteLine("Maximum Downloads\t\t" + mdownloadsN + "\t\t" + mdownloadsK);
		}

		static public void SendOneBroadCast(int size, bool trace = false)
		{
			Console.WriteLine("***SendOneBroadCast***");
			Random r = new Random();
			int randNum = (int)(size * r.NextDouble());
			if(trace) Console.WriteLine("Broadcasting to index " + randNum);
			Stopwatch sw = Stopwatch.StartNew();
			_normalNodes[randNum].BroadCast("msg");
			tnormal = sw.ElapsedTicks;
			_kademliaNodes[randNum].BroadCast("msg");
			tkademlia = sw.ElapsedTicks - tnormal;

			utotalN = utotalK = dtotalN = dtotalK = muploadsN = muploadsK = mdownloadsN = mdownloadsK = 0;
			for (int i = 0; i < size; i++)
			{
				int temp;
				temp = _normalNodes[i].GetUploads();
				if (temp > muploadsN) muploadsN = temp;
				utotalN += temp;
				temp = _normalNodes[i].GetDownloads();
				if (temp > mdownloadsN) mdownloadsN = temp;
				dtotalN += temp;
				temp = _kademliaNodes[i].GetUploads();
				if (temp > muploadsK) muploadsK = temp;
				utotalK += temp;
				temp = _kademliaNodes[i].GetDownloads();
				if (temp > mdownloadsK) mdownloadsK = temp;
				dtotalK += temp;
			}
		}


		static public void SendMultipleBroadCasts(int size, int sendCount, bool trace = false)
		{
			Console.WriteLine("***SendMultipleBroadCasts***");
			Random r = new Random();
			Stopwatch sw = Stopwatch.StartNew();
			long elapsedTime = sw.ElapsedTicks;
			for (int i = 0; i < sendCount; i++)
			{
				int randNum = (int)(size * r.NextDouble());
				if (trace) Console.WriteLine("Broadcasting to index " + randNum);
				_normalNodes[randNum].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tnormal = elapsedTime > tnormal ? elapsedTime : tnormal;
				_kademliaNodes[randNum].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tkademlia = elapsedTime > tkademlia ? elapsedTime : tkademlia;
			}

			utotalN = utotalK = dtotalN = dtotalK = muploadsN = muploadsK = mdownloadsN = mdownloadsK = 0;
			for (int i = 0; i < size; i++)
			{
				int temp;
				temp = _normalNodes[i].GetUploads();
				if (temp > muploadsN) muploadsN = temp;
				utotalN += temp;
				temp = _normalNodes[i].GetDownloads();
				if (temp > mdownloadsN) mdownloadsN = temp;
				dtotalN += temp;
				temp = _kademliaNodes[i].GetUploads();
				if (temp > muploadsK) muploadsK = temp;
				utotalK += temp;
				temp = _kademliaNodes[i].GetDownloads();
				if (temp > mdownloadsK) mdownloadsK = temp;
				dtotalK += temp;
			}
		}

		static public void SendAllBroadCasts(int size, bool trace = false)
		{
			Console.WriteLine("***SendAllBroadCasts***");
			Random r = new Random();
			Stopwatch sw = Stopwatch.StartNew();
			long elapsedTime = sw.ElapsedTicks;
			for (int i = 0; i < size; i++)
			{
				if (trace) Console.WriteLine("Broadcasting to index " + i);
				_normalNodes[i].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tnormal = elapsedTime > tnormal ? elapsedTime : tnormal;
				_kademliaNodes[i].BroadCast("msg");
				elapsedTime = sw.ElapsedTicks - elapsedTime;
				tkademlia = elapsedTime > tkademlia ? elapsedTime : tkademlia;
			}

			utotalN = utotalK = dtotalN = dtotalK = muploadsN = muploadsK = mdownloadsN = mdownloadsK = 0;
			for (int i = 0; i < size; i++)
			{
				int temp;
				temp = _normalNodes[i].GetUploads();
				if (temp > muploadsN) muploadsN = temp;
				utotalN += temp;
				temp = _normalNodes[i].GetDownloads();
				if (temp > mdownloadsN) mdownloadsN = temp;
				dtotalN += temp;
				temp = _kademliaNodes[i].GetUploads();
				if (temp > muploadsK) muploadsK = temp;
				utotalK += temp;
				temp = _kademliaNodes[i].GetDownloads();
				if (temp > mdownloadsK) mdownloadsK = temp;
				dtotalK += temp;
			}
		}

		static public bool CreateNormalNode(int id)
		{
			foreach (NormalNode node in _normalNodes)
			{
				if (node.GetId() == id)
					return false;
			}

			NormalNode newNode = new NormalNode(id, _normalNodes);
			foreach (NormalNode node in _normalNodes)
				node.NewNode(newNode);
			_normalNodes.Add(newNode);
			return true;
		}

		static public bool CreateKademliaNode(int id)
		{
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (node.GetId() == id)
					return false;
			}

			KademliaNode newNode = new KademliaNode(id, _kademliaNodes);
			foreach (KademliaNode node in _kademliaNodes)
				node.NewNode(newNode);
			_kademliaNodes.Add(newNode);
			return true;
		}

		static public bool AllKademliaNodeTouched(bool trace = false)
		{
			bool success = true;
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (!node.Touched())
				{
					if(trace) Console.WriteLine("Node of id " + node.GetId() + " is not touched");
					success = false;
				}
			}

			return success;
		}

		static public bool AllNormalNodeTouched(bool trace = false)
		{
			bool success = true;
			foreach (NormalNode node in _normalNodes)
			{
				if (!node.Touched())
				{
					if(trace) Console.WriteLine("Node of id " + node.GetId() + " is not touched");
					success = false;
				}
			}

			return success;
		}
	}
}
