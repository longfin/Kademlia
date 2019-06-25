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
		private int _find_request;

		public KademliaNode(int id) : base(id)
		{
			_table = new Bucket[k_TableSize];
			_find_request = -1;
			for (int i = 0; i < k_TableSize; i++)
				_table[i] = new Bucket();
		}

		// Initial connect to bootstrap node
		public void Bootstrap(KademliaNode bootstrapNode)
		{
			if (bootstrapNode is null)
				return;

			// This implementation will get information of all nodes
			// on the network
			Update(bootstrapNode);
			foreach (KademliaNode node in bootstrapNode.GetAllNodes(this, 0))
				Update(node);

			// Implementation in this comment is the one in the
			// formal Kademlia
			//FindNode(_id, bootstrapNode);
		}

		public void Update(KademliaNode node)
		{
			if (node.GetId() == _id) return;
			int plength = CommonPrefixLength(_id, node.GetId());
			_table[plength].Add(node);
		}

		public override void Close()
		{
			List<KademliaNode> nodes = new List<KademliaNode>();
			foreach (Bucket bucket in _table)
			{
				foreach (KademliaNode node in bucket._contents)
					nodes.Add(node);
			}

			BroadCastClose(this, nodes, 0);
			_alive = false;
		}

		private List<KademliaNode> GetAllNodes(KademliaNode remote, int prefixLength)
		{
			List<KademliaNode> nodes = new List<KademliaNode>();
			nodes.Add(this);

			KademliaNode target;
			for (int i = prefixLength; i < k_TableSize; i++)
			{
				target = _table[i].GetRandomNode();
				if (target == null)
					continue;
				nodes.AddRange(target.GetAllNodes(remote, i + 1));
			}

			Update(remote);

			return nodes;
		}

		private void BroadCastClose(KademliaNode close, List<KademliaNode> nodes, int prefixLength)
		{
			if (!_alive)
				return;

			if (this != close)
			{
				int plength = CommonPrefixLength(_id, close._id);
				_table[plength].Remove(close);

				foreach (KademliaNode node in nodes)
					Update(node);
			}

			KademliaNode target;
			for (int i = prefixLength; i < k_TableSize; i++)
			{
				target = _table[i].GetRandomNode();
				if (target == null)
					continue;

				target.BroadCastClose(close, nodes, i + 1);
			}
		}

		public void RemoveNode(KademliaNode node)
		{
			int plength = CommonPrefixLength(_id, node.GetId());
			_table[plength].Remove(node);
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

		public void PingToTable()
		{
			List<KademliaNode> nodes = new List<KademliaNode>();
			foreach (Bucket bucket in _table)
			{
				foreach (KademliaNode node in bucket._contents)
					nodes.Add(node);
			}

			foreach (KademliaNode node in nodes)
				node.Ping(this);
		}

		public void PingAll()
		{
			BroadCastPing(this, 0);
		}

		private void BroadCastPing(KademliaNode remote, int prefixLength)
		{
			if (!_alive)
				return;

			KademliaNode target;
			for (int i = prefixLength; i < k_TableSize; i++)
			{
				target = _table[i].GetRandomNode();
				if (target == null || target == remote)
					continue;
				target.BroadCastPing(this, i + 1);
				target.Ping(remote);
			}
		}

		private void Ping(KademliaNode remote)
		{
			if(_alive)
				Update(remote);
			// actually if node is dead, no respond and cause timeout exception
			remote.Pong(this, _alive);
		}

		private void Pong(KademliaNode remote, bool alive)
		{
			if (alive)
				Update(remote);
			else
			{
				RemoveNode(remote);
			}
		}

		private void QuerryNeighbours(int target_id)
		{
			List<KademliaNode> neighbours = FindNeighbours(target_id);
			for (int i = 0; i < k_FindConcurrency && i < neighbours.Count; i++)
				neighbours[i].SendFindNode(this, target_id);
		}

		private void FindNode(int target_id, KademliaNode via_node = null)
		{
			_find_request = target_id;
			if (via_node is null)
				QuerryNeighbours(target_id);
			else
				via_node.SendFindNode(this, target_id);
			_find_request = -1;
		}

		private void SendFindNode(KademliaNode remote, int target_id)
		{
			Update(remote);
			List<KademliaNode> found = FindNeighbours(target_id);
			remote.SendNeighbours(this, found);
		}

		private List<KademliaNode> FindNeighbours(KademliaNode target, int k=Bucket.BucketSize)
		{
			return FindNeighbours(target._id, k);
		}

		private List<KademliaNode> FindNeighbours(int target_id, int k = Bucket.BucketSize)
		{
			// return k * 2 closest nodes
			List<KademliaNode> sorted = SortTableByDistance(target_id);
			List<KademliaNode> nodes = new List<KademliaNode>();
			foreach (KademliaNode node in sorted)
			{
				if (node._id != target_id)
				{
					nodes.Add(node);
					if (nodes.Count >= k * 2)
						break;
				}
			}

			return nodes;
		}

		private void SendNeighbours(KademliaNode remote, List<KademliaNode> neighbours)
		{
			// receive
			List<KademliaNode> nodes = new List<KademliaNode>();
			foreach (KademliaNode node in neighbours)
			{
				if (node != this && !IsInTable(node))
				{
					nodes.Add(node);
				}
			}

			if (_find_request != -1)
			{
				nodes = SortByDistance(nodes, _id);

				foreach (KademliaNode node in nodes)
				{
					if (node != this)
						node.Ping(this);
				}

				List<KademliaNode> closest_candidate = FindNeighbours(_find_request);
				KademliaNode closest_known = closest_candidate.Count == 0 ? null : closest_candidate[0];
				for (int i = 0; i < k_FindConcurrency && i < nodes.Count; i++)
				{
					if (closest_known != null && CommonPrefixLength(nodes[i]._id, _find_request) > CommonPrefixLength(closest_known._id, _find_request))
						nodes[i].SendFindNode(this, _find_request);
				}
			}

		}

		private bool IsInTable(KademliaNode node)
		{
			foreach (Bucket bucket in _table)
			{
				if (bucket._contents.Contains(node))
					return true;
			}

			return false;
		}

		private List<KademliaNode> SortTableByDistance(int target_id)
		{
			// sort whole table from closest to farthest
			// currently, algorithm is slow so FIXME
			List<KademliaNode> nodes = new List<KademliaNode>();
			foreach (Bucket bucket in _table)
			{
				foreach (KademliaNode node in bucket._contents)
					nodes.Add(node);
			}

			nodes = SortByDistance(nodes, target_id);
			for (int i = 1; i < nodes.Count; i++)
			{
				if (CommonPrefixLength(nodes[i - 1]._id, target_id) < CommonPrefixLength(nodes[i]._id, target_id))
					throw new Exception("Sorting not correct");
			}
			return nodes;
		}

		private static List<KademliaNode> SortByDistance(List<KademliaNode> nodes, int target_id)
		{
			// sort list from closest to farthest
			return nodes.OrderBy(node => (32 - CommonPrefixLength(node._id, target_id))).ToList();
		}


		public override bool BroadCast(string msg)
		{
			_touched = true;
			_log.Add("Broadcast message " + '"' + msg + '"');
			Transfer(this, msg, 0);
			return false;
		}

		private void Transfer(KademliaNode remote, string msg, int prefixLength)
		{
			if (!_alive)
				return;

			if(this != remote)
				Update(remote);

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
				target.Transfer(this, msg, i + 1);
			}

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


