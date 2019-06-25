using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	public class Kademlia
	{
		private const int MAX_TRIES = 1 << 10;
		static private List<NormalNode> _normalNodes;
		static private List<KademliaNode> _kademliaNodes;
		static private long tnormal, tkademlia;
		static private int utotalN, utotalK, dtotalN, dtotalK, muploadsN, muploadsK, mdownloadsN, mdownloadsK;
		static void Main(string[] args)
		{
			int NODECOUNT = 1 << 8;

			_normalNodes = new List<NormalNode>();
			_kademliaNodes = new List<KademliaNode>();

			for (int i = 0; i < NODECOUNT; i++)
			{
				CreateNormalNode(i);
				CreateKademliaNode(i);
			}

			PingAllKademliaNodes(NODECOUNT);


			//HaltRandomNode(NODECOUNT, haltCount: 15, trace: true);

			SendOneBroadCast(NODECOUNT, trace: true);
			//SendMultipleBroadCasts(NODECOUNT, 2000);
			//SendAllBroadCasts(NODECOUNT);

			//DisplayAllTable();

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

		static public void SendOneBroadCast(int size, int target = -1, bool trace = false)
		{
			Console.WriteLine("***SendOneBroadCast***");

			Random r = new Random();
			int randNum = target == -1 ? (int)(size * r.NextDouble()) : target;
			int tries;
			for (tries = 0; target == -1 & tries < MAX_TRIES; tries++)
			{
				if (!_kademliaNodes[randNum].Respond())
					randNum = (int)(size * r.NextDouble());
				else
					break;
			}

			if (tries >= MAX_TRIES)
			{
				if (trace) Console.WriteLine("Broadcast failed... cannot find alive node.");
				return;
			}

			if(trace) Console.WriteLine("Broadcasting to index {0} with {1} tries.", randNum, tries + 1);
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

			KademliaNode newNode = new KademliaNode(id);
			if(id != 0)
				newNode.Bootstrap(_kademliaNodes[0]);
			_kademliaNodes.Add(newNode);
			return true;
		}

		static public void DisplayAllTable()
		{
			foreach (KademliaNode node in _kademliaNodes)
			{
				Console.WriteLine("Node {0}", node.GetId());
				Console.WriteLine(node.PrintTable());
			}
		}

		static public bool AllKademliaNodeTouched(bool trace = false)
		{
			bool success = true;
			foreach (KademliaNode node in _kademliaNodes)
			{
				if (node.Respond() && !node.Touched())
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

		static public void HaltRandomNode(int size, int haltCount = 1, bool trace = false)
		{
			Random r = new Random(new Random().Next());
			for (int i = 0; i < haltCount; i++)
			{
				int randNum = (int)(size * r.NextDouble());
				if (!_kademliaNodes[randNum].Respond())
				{
					if (trace) Console.WriteLine("Node at {0} already halted", randNum);
					continue;
				}

				if (trace) Console.WriteLine("Halt node of index " + randNum);
				_kademliaNodes[randNum].Halt();
			}

			for(int i = 0; i < 1; i++) PingAllKademliaNodes(size);
		}

		public static void PingAllKademliaNodes(int size)
		{
			for (int i = 0; i < size; i++)
			{
				_kademliaNodes[i].BroadCastPing();
			}
		}
	}
}
