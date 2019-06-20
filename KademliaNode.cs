using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	class KademliaNode : Node
	{
		private Bucket[] _table;

		public KademliaNode(int id, List<KademliaNode> nodes) : base(id)
		{
			_table = new Bucket[32];
			for (int i = 0; i < 32; i++)
				_table[i] = new Bucket();

			foreach (KademliaNode node in nodes)
				NewNode(node);
		}

		public void NewNode(KademliaNode node)
		{
			if (node.GetId() == _id) return;
			int plength = CommonPrefixLength(_id, node.GetId());
			_table[plength].Add(node);
		}

		public void RemoveNode(KademliaNode node)
		{
			//int plength = CommonPrefixLength(_id, node.GetId());
		}

		private int CommonPrefixLength(int sourceId, int destId)
		{
			int xor = sourceId ^ destId;
			int mask = (1 << 31);
			int length = 0;
			while ((mask & xor) == 0 && mask != 0)
			{
				length++;
				mask >>= 1;
			}

			return length;
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
			for (int i = prefixLength; i < 32; i++)
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
			return true;
		}
	}
}
