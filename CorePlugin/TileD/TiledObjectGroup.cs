/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Duality;
using Duality.Drawing;

namespace TileD_Plugin.TileD
{	
	/// <summary>
	/// Description of TiledObjectGroup.
	/// </summary>
	public class TiledObjectGroup
	{
		public string Name {get;set;}
		public int W {get;set;}
		public int H {get;set;}
		public TiledPropertySet Properties {get;set;}
		public Dictionary<string, TiledObject> Objects {get;set;}
		public ColorRgba Color {get;set;}
		public bool Visible {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledObjectGroup()
		{
			Name = "UnnamedObjectGroup";
			Objects = new Dictionary<string, TiledObject>();
			Properties = new TiledPropertySet();
		}
	}
}
