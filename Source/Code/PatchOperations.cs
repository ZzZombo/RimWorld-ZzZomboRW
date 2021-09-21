using System.Xml;
using Verse;
using RimWorld;

namespace ZzZomboRW.Framework
{
	public class PatchOperationAddNodeWithDefault: PatchOperationPathed
	{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649 // Field is never assigned to
		private string name;
		private XmlContainer value;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Field is never assigned to
		public override bool ApplyWorker(XmlDocument xml)
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
