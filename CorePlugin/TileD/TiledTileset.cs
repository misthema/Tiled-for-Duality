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
		public static int TerrainCount {get;set;}
		public int TerrainID {get;set;}
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
		
		public void LoadProperties( XElement node )
		{
			Properties.Extend( node );
		}
		
		public void LoadOffset( XElement node )
		{
			if( node.HasAttributes )
			{
				var tileoff = TileOffset;
								
				foreach( var attribute in node.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "x":
							tileoff.X = int.Parse( attribute.Value, System.Globalization.NumberStyles.Integer );
							break;
							
						case "y":
							tileoff.Y = int.Parse( attribute.Value, System.Globalization.NumberStyles.Integer );
							break;
				
						default:
							Log.Editor.WriteWarning("Attribute {0} not supported in tileset offset.", attribute.Name);
							break;
					}
				}
				
				TileOffset = tileoff;
			}
				
		}

		public void LoadImage( XElement node )
		{
			if( !node.HasAttributes || node.Name != "image" )
				return;
			
			string source = TiledHelper.GetAttribute(node, "source"); //node.Attribute("source").Value;
			int width = int.Parse( TiledHelper.GetAttribute(node, "width"), System.Globalization.NumberStyles.Integer );
			int height = int.Parse( TiledHelper.GetAttribute(node, "height"), System.Globalization.NumberStyles.Integer );
			
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
			/*if( !node.Name.Equals("terraintypes") )
			{
				Log.Editor.WriteWarning("Loading TerrainType failed!");
				return;
			}*/
			
			if( node.HasElements )
			{
				foreach( var element in node.Elements() )
				{
					if( element.Name != "terrain" ) continue;
					if( !element.HasAttributes ) continue;
					
					var terrainType = new TiledTerrainType();
					terrainType.TerrainID = TiledTerrainType.TerrainCount++;
					terrainType.Name = TiledHelper.GetAttribute(element, "name"); //node.Attribute("name").Value;
					terrainType.TileID = int.Parse( TiledHelper.GetAttribute(element, "tile"), System.Globalization.NumberStyles.Integer );
					
					TerrainTypes.Add( terrainType.TileID, terrainType );
				}
			}
			else
				Log.Editor.WriteWarning("Node <{0}> has no elements. (Should have <terrain> nodes!)", node.Name);
		}
		
		public void LoadTerrainTile( XElement tileNode )
		{

			if( tileNode.Name != "tile" )
			{
				Log.Editor.WriteWarning("Node is not a <tile>, it is <{0}>.", tileNode.Name );
				return;
			}
			if( !tileNode.HasAttributes )
			{
				Log.Editor.WriteWarning("Node <{0}> does not have attribues. (Expected a <tile> node with attributes)", tileNode.Name );
				return;
			}
			
			var idStr = TiledHelper.GetAttribute(tileNode, "id");
			var tileID = int.Parse( string.IsNullOrEmpty(idStr) ? "0" : idStr, System.Globalization.NumberStyles.Integer );
			string terrainStr = TiledHelper.GetAttribute(tileNode, "terrain");
			string[] terrain = terrainStr.Split(',');
			
			TiledTerrainType terrainType = new TiledTerrainType();
			
			if( TerrainTypes.ContainsKey(tileID) )
			{
				terrainType = TerrainTypes[tileID];
			}
			
			if( terrain != null && terrain.Length > 0 )
			{
				terrainType.Data = new int[4];
				for( int i = 0; i < terrain.Length; i++ )
				{
					if( !string.IsNullOrEmpty( terrain[i] ) )
						terrainType.Data[i] = int.Parse( terrain[i], System.Globalization.NumberStyles.Integer );
					else
						terrainType.Data[i] = -1;
				}
			}
			else
				Log.Editor.WriteWarning("No terrain data in <{0}.{1}>.", tileNode.Parent.Parent.Name, tileNode.Parent.Name );

		}
	}
}
