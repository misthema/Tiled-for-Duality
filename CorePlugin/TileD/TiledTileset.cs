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
using Duality.Drawing;
using Duality.Properties;

namespace TileD_Plugin.TileD
{
	public struct TiledTerrainType
	{
		public string Name {get;set;}
		public int[] Data {get;set;}
		public int TileID {get;set;}
	}
	
	/// <summary>
	/// Description of TiledTileset.
	/// </summary>
	public class TiledTileset
	{
		public string Name {get;set;}
		public ContentRef<Material> Image {get;set;}
		public int W {get;set;}
		public int H {get;set;}
		public int WPixel {get;set;}
		public int HPixel {get;set;}
		public int TileW {get;set;}
		public int TileH {get;set;}
		public int FirstGID {get;set;}
		public int LastGID {get;set;}
		public int Margin {get;set;}
		public int Spacing {get;set;}
		public Vector2 TileOffset {get;set;}
		public TiledPropertySet Properties {get;set;}
		public Dictionary<int, TiledPropertySet> TileProperties {get;set;}
		public int TileCount {get;set;}
		public Dictionary<int, TiledTerrainType> TerrainTypes {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledTileset()
		{
			Name = "UnnamedTileset";
			TileOffset = new Vector2(0, 0);
			Properties = new TiledPropertySet();
			TileProperties = new Dictionary<int, TiledPropertySet>();
			TerrainTypes = new Dictionary<int, TiledTerrainType>();
			TileCount = 0;
		}
		
		public bool Contains( int gid )
		{
			if( gid >= FirstGID && gid < FirstGID + TileCount )
				return true;
			
			return false;
		}

		public void LoadImage( XElement node )
		{
			if( !node.HasAttributes || node.Name != "image" )
				return;
			
			string source = node.Attribute("source").Value;
			int width = int.Parse( node.Attribute("width").Value, System.Globalization.NumberStyles.Integer );
			int height = int.Parse( node.Attribute("height").Value, System.Globalization.NumberStyles.Integer );
			
			WPixel = width;
			HPixel = height;
			W = WPixel / TileW;
			H = HPixel / TileH;
			
			var split = source.Split('/');
			string tilesetName = split[ split.Length-1 ].Replace(".png", "");
			
			Log.Editor.Write("Tileset name: {0}", tilesetName);
			Log.Editor.Write("Found Materials:");
			foreach( var res in ContentProvider.GetAvailableContent<Material>() )
			{
				
				Log.Editor.Write("    {0}", res.FullName);
				
				if( !res.FullName.Contains(tilesetName) ) continue;
				
				/*Log.Editor.Write("    Using {0}, creating Texture...", res.FullName);
				Texture tex = new Texture(
					res.Res,
					TextureSizeMode.NonPowerOfTwo
				);
				
				Log.Editor.Write("    Using {0}, creating BatchInfo...", res.FullName);
				BatchInfo binf = new BatchInfo(
					DrawTechnique.Mask,
					ColorRgba.White,
					new ContentRef<Texture>(tex)
				);*/
				
				Log.Editor.Write("    Using {0}, loading Material...", res.FullName);
				Image = res;
				//Image = res;
				
				break;
			}
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
					
					terrainType.Data = new int[4];
					for( int i = 0; i < 4; i++ )
					{
						terrainType.Data[i] = int.Parse( terrain[i], System.Globalization.NumberStyles.Integer );
					}
				}
			}
		}
	}
}
