/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using Duality;
using Duality.Resources;

namespace TileD_Plugin.TileD
{
	public struct TiledTerrainType
	{
		public string Name {get;set;}
		public bool[] Data {get;set;}
		public int TileID {get;set;}
	}
	
	/// <summary>
	/// Description of TiledTileset.
	/// </summary>
	public class TiledTileset
	{
		public static List<TiledTileset> Tilesets {get;set;}
		
		public string Name {get;set;}
		public ContentRef<Material> Image {get;set;}
		public int W {get;set;}
		public int H {get;set;}
		public int TileW {get;set;}
		public int TileH {get;set;}
		public int FirstGID {get;set;}
		public int LastGID {get;set;}
		public int Margin {get;set;}
		public int Spacing {get;set;}
		public Vector2 Offset {get;set;}
		public TiledPropertySet Properties {get;set;}
		public Dictionary<int, TiledPropertySet> TileProperties {get;set;}
		public int TileCount {get;set;}
		public Dictionary<int, TiledTerrainType> TerrainTypes {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledTileset()
		{
			Name = "UnnamedTileset";
			Offset = new Vector2();
			Properties = new TiledPropertySet();
			TileProperties = new Dictionary<int, TiledPropertySet>();
			TerrainTypes = new Dictionary<int, TiledTerrainType>();
			TileCount = 0;
			
			if( TiledTileset.Tilesets == null )
				TiledTileset.Tilesets = new List<TiledTileset>();
			else
				TiledTileset.Tilesets.Add(this);
		}
		
		public bool Contains( int gid )
		{
			if( FirstGID < gid && FirstGID + TileCount > gid )
				return true;
			
			return false;
		}
		
		public static TiledTileset FindByGID( int gid )
		{
			TiledTileset tileset = Tilesets[0];
			int i = 0;
			
			while( tileset != null )
			{
				if( tileset.Contains(gid) )
					return tileset;
				
				i++;
				tileset = Tilesets[i];
			}
			
			return null;
		}
		
		public void LoadImage( XElement node )
		{
			
		}
		
		public void LoadTerrainTypes( XElement node )
		{
			if( node.HasElements )
			{
				foreach( var element in node.Elements() )
				{
					if( !element.HasAttributes ) continue;
					
					var terrainType = new TiledTerrainType();
					terrainType.Name = node.Attribute("name").Value;
					terrainType.TileID = int.Parse( node.Attribute("tile").Value, System.Globalization.NumberStyles.Integer );
					
					TerrainTypes.Add( terrainType.TileID, terrainType );
				}
			}
		}
		
		public void LoadTerrainTiles( XElement node )
		{
			if( node.HasElements )
			{
				foreach( var element in node.Elements() )
				{
					if( element.Name != "tile" ) continue;
					if( !element.HasAttributes ) continue;
					
					var tileID = int.Parse( node.Attribute("id").Value, System.Globalization.NumberStyles.Integer );
					string[] terrain = node.Attribute("terrain").Value.Split(',');
					
					var terrainType = TerrainTypes[tileID];
					
					terrainType.Data = new bool[4];
					for( int i = 0; i < 4; i++ )
					{
						terrainType.Data[i] = (terrain[i] == "0");
					}
				}
			}
		}
	}
}
