/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Duality;
using Duality.Resources;

namespace TileD_Plugin.TileD
{
	/// <summary>
	/// Description of TiledImageLayer.
	/// </summary>
	public class TiledImageLayer
	{
		public string Name {get;set;}
		public int X {get;set;}
		public int Y {get;set;}
		public float Opacity {get;set;}
		public bool Visible {get;set;}
		public ContentRef<Material> Image {get;set;}
		public TiledPropertySet Properties {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledImageLayer()
		{
			Name = "UnnamedImageLayer";
			Properties = new TiledPropertySet();
		}
	}
}
