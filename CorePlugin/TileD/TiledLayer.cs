/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;
using System.Xml.Linq;
using Duality;




namespace TileD_Plugin.TileD
{
	/// <summary>
	/// Description of TiledLayer.
	/// </summary>
	public class TiledLayer
	{
		public string Name {get;set;}
		public int W {get;set;}
		public int H {get;set;}
		public float Opacity {get;set;}
		public bool Visible {get;set;}
		public TiledPropertySet Properties {get;set;}
		public int[] Tiles {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledLayer()
		{
			Name = "UnnamedLayer";
			W = 0;
			H = 0;
			Properties = new TiledPropertySet();
			Visible = true;
		}
		
		internal bool validateTiles( int x, int y )
		{
			if( Tiles == null || Tiles.Length == 0 )
			{
				Log.Editor.WriteWarning("Layer {0} tile array is not initialized.", Name);
				return false;
			}
			
			if( x < 0 || x > W
			   || y < 0 || y > H )
			{
				Log.Editor.WriteWarning("Trying to access a tile out of bounds.");
				return false;
			}
			
			return true;
		}
		
		public void SetTile( int x, int y, int newID )
		{
			if( validateTiles(x, y) )
				Tiles[y * W + x] = newID;
		}
		
		public int GetTile( int x, int y )
		{
			return !validateTiles(x, y) ? -1 : Tiles[y * W + x];
		}
		
		public void LoadTiles( XElement node )
		{
			if( !node.HasElements )
			{
				Log.Editor.WriteWarning("Layer '{0}' has no tile data?", this.Name);
				return;
			}
			
			Tiles = new int[W * H];
			
			// Load tiles from <data>'s elements
			int i = 0;
			foreach( var element in node.Elements() )
			{
				if( element.Name != "tile" )
					continue;
				
				foreach( var attribute in element.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "gid":
							var gid = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
							int x = i % W;
							int y = i / W;
							
							SetTile(x, y, gid);
							i++;
							break;
							
						default:
							Log.Editor.Write("Attribute {0} is not supported in layer tiles.", attribute.Name);
							break;
					}
				}
			}
		}
		
		public void LoadProperties( XElement node )
		{
			Properties.Extend( node );
		}
	}
}
