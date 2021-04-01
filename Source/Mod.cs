using System.Xml;
using Verse;
using RimWorld;

internal static class MOD
{
	public const string NAME = "ZzZombo's Framework";
}

namespace ZzZomboRW
{
	public class PatchOperationAddNode: PatchOperationPathed
	{
		private string name;
		private XmlContainer value;
		protected override bool ApplyWorker(XmlDocument xml)
		{
			var result = false;
			var nodes = xml.SelectNodes(this.xpath);
			foreach(var obj in nodes)
			{
				if(obj is XmlNode xmlNode)
				{
					if(name.NullOrEmpty())
					{
						foreach(var child in this.value.node.ChildNodes)
						{
							xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode((XmlNode)child, true));
						}
						result = true;
						continue;
					}
					XmlNode container = xmlNode[this.name];
					if(container == null)
					{
						container = xmlNode.OwnerDocument.CreateElement(this.name);
						xmlNode.AppendChild(container);
					}
					foreach(var child in this.value.node.ChildNodes)
					{
						container.AppendChild(xmlNode.OwnerDocument.ImportNode((XmlNode)child, true));
					}
					result = true;
				}
			}
			return result;
		}
	}
}
