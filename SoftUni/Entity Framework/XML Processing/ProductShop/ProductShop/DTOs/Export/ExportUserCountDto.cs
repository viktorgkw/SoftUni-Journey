﻿namespace ProductShop.DTOs.Export
{
	using System.Xml.Serialization;

	[XmlType("Users")]
	public class ExportUserCountDto
	{
		[XmlElement("count")]
		public int Count { get; set; }

		[XmlArray("users")]
		public UserInfo[] Users { get; set; } = null!;
	}
}