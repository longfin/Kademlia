using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kademlia
{
	class NormalNode : Node
	{
		private List<NormalNode> _table;

		public NormalNode(int id, List<NormalNode> nodes) : base(id)
		{
			_table = new List<NormalNode>();

			foreach (NormalNode node in nodes)
				NewNode(node);
		}
		public void NewNode(NormalNode node)
		{
			if (node.GetId() == _id) return;
			_table.Add(node);
		}

		public void RemoveNode(NormalNode node)
		{
			_table.Remove(node);
		}

		public override bool BroadCast(string msg)
		{
			_touched = true;
			_log.Add("Broadcast message " + '"' + msg + '"');
			foreach (NormalNode node in _table)
			{
				_uploads++;
				node.Transfer(msg);
			}

			return true;
		}

		private void Transfer(string msg)
		{
			_downloads++;
			_touched = true;
			_log.Add("Received : " + '"' + msg + '"');
		}

	}
}
