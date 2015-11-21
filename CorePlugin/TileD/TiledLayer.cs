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
		public int[,] Tiles {get;set;}
		public TiledTileset Tileset {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledLayer(int w, int h)
		{
			Name = "UnnamedLayer";
			W = w;
			H = h;
			Tiles = new int[w, h];
			Properties = new TiledPropertySet();
		}
		
		internal bool validateTiles( int x, int y )
		{
			if( Tiles == null )
			{
				Log.Editor.WriteWarning("Trying to access a null layer (doesn't exist).");
				return false;
			}
			
			if( x < 0 || x > Tiles.GetUpperBound(0)
			   || y < 0 || y > Tiles.GetUpperBound(1) )
			{
				Log.Editor.WriteWarning("Trying to access a tile out of bounds.");
				return false;
			}
			
			return true;
		}
		
		public void SetTile( int x, int y, int newID )
		{
			if( validateTiles(x, y) )
				Tiles[x, y] = newID;
		}
		
		public int GetTile( int x, int y )
		{
			return !validateTiles(x, y) ? -1 : Tiles[x, y];
		}
		
		public void LoadTiles( XElement node )
		{
			if( !node.HasElements )
			{
				Log.Editor.WriteWarning("Layer '{0}' has no tile data?", this.Name);
				return;
			}
			
			// Load tiles from <data>'s elements
			int i = 0;
			foreach( var tile in node.Elements() )
			{
				if( tile.Name != "tile" ) continue;
				if( !tile.HasAttributes ) continue;
				
				var gid = int.Parse(tile.Attribute("gid").Value, System.Globalization.NumberStyles.Integer);
				int x = i % W;
				int y = i / W;
				
				SetTile(x, y, gid);
				i++;
			}
		}
	}
}
