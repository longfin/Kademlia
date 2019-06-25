using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	public class KademliaNode : Node
	{
		private const int k_FindConcurrency = 3;
		private const int k_TableSize = 32;
		private Bucket[] _table;

		public KademliaNode(int id) : base(id)
		{
			_table = new Bucket[k_TableSize];
			for (int i = 0; i < k_TableSize; i++)
				_table[i] = new Bucket();
		}

		// Initial connect to bootstrap node
		public void Connect(KademliaNode bootstrapNode)
		{
			if (bootstrapNode is null)
				return;

			AddNode(bootstrapNode);
			foreach (KademliaNode node in bootstrapNode.FindNode(this))
				AddNode(node);
		}

		public void AddNode(KademliaNode node)
		{
			if (node.GetId() == _id) return;
			int plength = CommonPrefixLength(_id, node.GetId());
			_table[plength].Add(node);
		}

		public void RemoveNode(KademliaNode node)
		{
			//int plength = CommonPrefixLength(_id, node.GetId());
		}

		public void Halt()
		{
			_alive = false;
		}

		private static int CommonPrefixLength(int sourceId, int destId)
		{
			int xor = sourceId ^ destId;
			int mask = (1 << 31);
			int length = 0;
			while (length < 32 && (mask & xor) == 0 && mask != 0)
			{
				length++;
				mask >>= 1;
			}

			return length;
		}

		private List<KademliaNode> FindNode(KademliaNode target)
		{
			List<KademliaNode> neighbours = new List<KademliaNode>();
			AddNeighbours(target, neighbours, new List<KademliaNode>());
			/*int index = 0;
			while (neighbours.Count < Bucket.BucketSize * 2 && index < neighbours.Count)
				neighbours[index++].AddNeighbours(target, neighbours);*/

			/*for (index = 0; index < neighbours.Count; index++)
				neighbours[index].AddNeighbours(target, neighbours);*/

			return neighbours;
		}

		private void AddNeighbours(KademliaNode target, List<KademliaNode> neighbours, List<KademliaNode> querry)
		{
			KademliaNode[] closest = GetCloseNodes(target._id, querry);
			for (int i = 0; i < k_FindConcurrency; i++)
			{
				if (closest[i] is null) break;

				querry.Add(closest[i]);
				closest[i].AddNeighbours(target, neighbours, querry);
			}

			for (int i = 0; i < k_FindConcurrency; i++)
			{
				if (closest[i] is null || neighbours.Count > Bucket.BucketSize * 2) break;
				if (neighbours.Contains(closest[i])) continue;

				neighbours.Add(closest[i]);
			}

			AddNode(target);
		}

		private KademliaNode[] GetCloseNodes(int target_id, List<KademliaNode> querry)
		{
			KademliaNode[] closeNodes = new KademliaNode[k_FindConcurrency];
			for (int i = 0; i < k_TableSize; i++)
			{
				foreach (KademliaNode node in _table[i]._contents)
				{
					if (querry.Contains(node) ||
						CommonPrefixLength(target_id, _id) > CommonPrefixLength(target_id, node._id) ||
						CommonPrefixLength(target_id, node._id) == 32)
						continue;

					if (closeNodes[0] is null || CommonPrefixLength(target_id, node._id) >= CommonPrefixLength(target_id, closeNodes[0]._id))
					{
						closeNodes[2] = closeNodes[1];
						closeNodes[1] = closeNodes[0];
						closeNodes[0] = node;
					}
					else if (closeNodes[1] is null || CommonPrefixLength(target_id, node._id) >= CommonPrefixLength(target_id, closeNodes[1]._id))
					{
						closeNodes[2] = closeNodes[1];
						closeNodes[1] = node;
					}
					else if (closeNodes[2] is null || CommonPrefixLength(target_id, node._id) >= CommonPrefixLength(target_id, closeNodes[2]._id))
					{
						closeNodes[2] = node;
					}
				}
			}

			return closeNodes;
		}

		public override bool BroadCast(string msg)
		{
			_touched = true;
			_log.Add("Broadcast message " + '"' + msg + '"');
			Transfer(msg, 0);
			return false;
		}

		private void Transfer(string msg, int prefixLength)
		{
			if (prefixLength != 0)
			{
				_touched = true;
				_log.Add("Received : " + '"' + msg + '"');
				_downloads++;
			}

			KademliaNode target;
			for (int i = prefixLength; i < k_TableSize; i++)
			{
				target = _table[i].GetRandomNode();
				if (target == null)
					continue;
				_log.Add("Transfer " + '"' + msg + '"' + " to Node " + target.GetId());
				_uploads++;
				target.Transfer(msg, i + 1);
			}

		}

		public bool Respond()
		{
			return _alive;
		}

		public string PrintTable()
		{
			string ret = "";
			for (int i = 0; i < k_TableSize; i++)
			{
				if (_table[i]._contents.Count == 0)
					continue;

				ret += "table entry " + i + " : ";
				foreach (KademliaNode node in _table[i]._contents)
					ret += node._id + ", ";

				ret += "\n";
			}

			return ret;
		}
	}
}
