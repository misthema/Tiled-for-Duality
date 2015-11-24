using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Duality;
using Duality.Drawing;
using Duality.Cloning;
using Duality.Editor;
using Duality.Properties;
using Duality.Resources;
using SnowyPeak.Duality.Plugin.Data.Resources;

namespace TileD_Plugin.TileD
{
	public enum RenderOrders {
		RightDown,
		RightUp,
		LeftDown,
		LeftUp,
		Default
	}
	
	/// <summary>
	/// Description of TiledMap.
	/// </summary>
	public class TiledMap : Component, IDisposable, ICmpRenderer
	{
		
		//public GameObject GameObj {get;set;}
		
		public RenderOrders RenderOrder  { get;set; }
		
		public Vector3 Pos {
			get;
			set;
		}
		
		public int W {
			get;
			set;
		}
		
		public int H {
			get;
			set;
		}
		
		public int TileW {get;set;}
		public int TileH {get;set;}
		
		public int FullW {
			get { return W * TileW; }
		}
		
		public int FullH {
			get { return H * TileH; }
		}
		
		public int NextObjectID { get;set; }
		
		public string Version {get;set;}
		public string Orientation {get;set;}
		
		public Dictionary<string, TiledTileset> Tilesets {get;set;}
		public Dictionary<string, TiledLayer> Layers {get;set;}
		public Dictionary<string, TiledObjectGroup> ObjectGroups {get;set;}
		public Dictionary<string, TiledImageLayer> ImageLayers {get;set;}
		
		private bool _loaded;
		
		public TiledMap(XmlData map)
		{
			//this.GameObj = obj;
			
			Tilesets = new Dictionary<string, TiledTileset>();
			Layers = new Dictionary<string, TiledLayer>();
			ObjectGroups = new Dictionary<string, TiledObjectGroup>();
			ImageLayers = new Dictionary<string, TiledImageLayer>();
			
			var root = map.XmlDocument.Root;
			XElement node;
			
			_loaded = false;
			
			
			foreach( var x in map.XmlDocument.Nodes() )
			{
				node = (XElement)x;
				
				switch( node.Name.LocalName )
				{
					case "map":
						ReadMapAttributes(node);
						
						// Load map data
						foreach( var y in node.Elements() )
						{
							node = (XElement)y;
							
							switch( node.Name.LocalName )
							{
								case "layer":
									TiledLayer layer = LoadLayer(node);
									if( layer != null ) Layers.Add(layer.Name, layer);
									
									break;
								case "tileset":
									TiledTileset tileset = LoadTileset(node);
									if( tileset != null ) Tilesets.Add(tileset.Name, tileset);
									
									break;
								case "objectgroup":
									TiledObjectGroup objGroup = LoadObjectGroup(node);
									if( objGroup != null ) ObjectGroups.Add(objGroup.Name, objGroup);
									
									break;
								case "imagelayer":
									TiledImageLayer imgLayer = LoadImageLayer(node);
									if( imgLayer != null ) ImageLayers.Add(imgLayer.Name, imgLayer);
									
									break;
							}
						}
						
						break;
					
				}
				
				Log.Editor.Write("Width: {0}, Height: {1}", W, H);
				Log.Editor.Write("Layer order:");
				foreach( var la in Layers.Values )
				{
					Log.Editor.Write("    {0}", la.Name );
				}
			}
			
			_loaded = true;
		}
		
		public void Clear()
		{
			Tilesets.Clear();
			Layers.Clear();
			ObjectGroups.Clear();
			ImageLayers.Clear();
		}
		
		void ReadMapAttributes(XElement node)
		{
			if( node == null || !node.HasAttributes )
			{
				Log.Editor.WriteError("Cannot read map attributes: node is NULL or has no attributes.");
				return;
			}
			
			Log.Editor.Write("Reading map attributes...");
			
			
			// Get not-so-important attributes
			Version = node.Attribute("version").Value;
			Orientation = node.Attribute("orientation").Value;
			NextObjectID = int.Parse(node.Attribute("nextobjectid").Value);
			
			switch( node.Attribute("renderorder").Value )
			{
				case "right-down":
					RenderOrder = RenderOrders.RightDown;
					break;
				case "right-up":
					RenderOrder = RenderOrders.RightUp;
					break;
				case "left-down":
					RenderOrder = RenderOrders.LeftDown;
					break;
				case "left-up":
					RenderOrder = RenderOrders.LeftUp;
					break;
				default:
					RenderOrder = RenderOrders.Default;
					break;
			}
			
			
			// Get map width and height (in tiles)
			W = int.Parse(node.Attribute("width").Value);
			H = int.Parse(node.Attribute("height").Value);
			
			// Tile sizes (in pixels)
			TileW = int.Parse(node.Attribute("tilewidth").Value);
			TileH = int.Parse(node.Attribute("tileheight").Value);
		}
		
		TiledLayer LoadLayer( XElement node )
		{
			if( node.HasAttributes )
			{
				Log.Editor.Write("Loading layer '{0}'...", node.Attribute("name").Value);
				
				// Width and height
				var w = int.Parse( node.Attribute("width").Value,
				                   System.Globalization.NumberStyles.Integer);
				var h = int.Parse( node.Attribute("height").Value,
				                   System.Globalization.NumberStyles.Integer);
				
				// New layer
				var layer = new TiledLayer(w, h);
				layer.Parent = this;
				
				// Name
				layer.Name = node.Attribute("name").Value;
				
				// Opacity
				layer.Opacity = node.Attribute("opacity") != null ? float.Parse(node.Attribute("opacity").Value, System.Globalization.NumberStyles.Float) : 1f;
				
				// Visible
				layer.Visible = node.Attribute("visible") != null ? false : true;
				
				// Tiledata and properties
				if( node.HasElements )
				{
					foreach( var element in node.Elements() )
					{
						switch( element.Name.LocalName )
						{
							case "properties":
								layer.Properties.Extend( element );
								break;
								
							case "data":
								layer.LoadTiles( element );
								break;
							default:
								Log.Editor.WriteWarning( "'{0}' is not supported in layers!", element.Name );
								break;
						}
					}
				}
				
				return layer;
			}
			
			return null;
		}
		
		TiledTileset LoadTileset( XElement node )
		{
			if( node.HasAttributes )
			{
				Log.Editor.Write("Loading tileset '{0}'...", node.Attribute("name").Value);
				
				var tileset = new TiledTileset();
				tileset.Parent = this;
				
				tileset.FirstGID = int.Parse( node.Attribute("firstgid").Value, System.Globalization.NumberStyles.Integer );
				
				tileset.Name = node.Attribute("name").Value;
				tileset.TileW = int.Parse( node.Attribute("tilewidth").Value, System.Globalization.NumberStyles.Integer );
				tileset.TileH = int.Parse( node.Attribute("tileheight").Value, System.Globalization.NumberStyles.Integer );
				tileset.TileCount = int.Parse( node.Attribute("tilecount").Value, System.Globalization.NumberStyles.Integer );
				
				if( node.HasElements )
				{
					foreach( var element in node.Elements() )
					{
						switch( element.Name.LocalName )
						{
							case "tileoffset":
								var tileoff = tileset.TileOffset;
								tileoff.X = int.Parse( element.Attribute("x").Value, System.Globalization.NumberStyles.Integer );
								tileoff.Y = int.Parse( element.Attribute("y").Value, System.Globalization.NumberStyles.Integer );
								tileset.TileOffset = tileoff;
								break;
								
							case "image":
								tileset.LoadImage( element );
								break;
								
							case "terraintypes":
								tileset.LoadTerrainTypes( element );
								break;
								
							case "tile":
								tileset.LoadTerrainTiles( node );
								break;
						}
					}
				}
				
				return tileset;
			}
			
			return null;
		}
		
		TiledObjectGroup LoadObjectGroup( XElement node )
		{
			// TODO: LoadObjectGroup and rendering
			Log.Editor.Write("Loading object group '{0}'...", node.Attribute("name").Value);
			
			//objGroup.Parent = this;
			
			return null;
		}
		
		TiledImageLayer LoadImageLayer( XElement node )
		{
			// TODO: LoadImageLayer and rendering
			Log.Editor.Write("Loading image layer '{0}'...", node.Attribute("name").Value);
			
			//imgLayer.Parent = this;
			
			return null;
		}
		
		public TiledTileset FindTilesetByGID( int gid )
		{
			foreach (var tileset in Tilesets.Values)
			{
				if( tileset.Contains(gid) )
					return tileset;
			}
			
			return null;
		}
		
		
		public void Draw( IDrawDevice device )
		{
			if( GameObj == null || !_loaded || Layers == null || Layers.Count == 0 )
				return;
			
			Vector3 tempPos = GameObj.Transform.Pos;
			float tempScale = 1f;
			device.PreprocessCoords( ref tempPos, ref tempScale );
			
			int halfMapW = W / 2;
			int halfMapH = H / 2;
			
			for (var i = Layers.Values.GetEnumerator(); i.MoveNext();) {
				
				var layer = i.Current;
				
				if (!layer.Visible)
					continue;
		
				for (int y = 0; y < H; y++) {
					
					for (int x = 0; x < W; x++) {
						
						// Is renderable tile available?
						int gid = layer.GetTile(x, y);
						if (gid <= 0)
							continue;
						
						// Get the correct tileset for this GID
						var tileset = FindTilesetByGID(gid);
						if (tileset == null)
							continue;
		
						// Remove tileset's FirstGID from the tile ID
						// so we get the correct position in the image.
						int tileX = (gid - tileset.FirstGID) % tileset.W;
						int tileY = (gid - tileset.FirstGID) / tileset.W;
						
						// Let 'em float...
						float tx = (float)tileX;
						float ty = (float)tileY;
						float tw = (float)TileW;
						float th = (float)TileH;
						float twp = (float)tileset.WPixel;
						float thp = (float)tileset.HPixel;
						
						var vertices = new VertexC1P3T2[4];
						var color = tileset.Image.Res.MainColor;
						var uvRatio = tileset.Image.Res.MainTexture.Res.UVRatio;
						
						// Texture coordinates
						var uvRect = new Rect(
							uvRatio.X * (tx * tw) / twp,
							uvRatio.Y * (ty * th) / thp,
							uvRatio.X * tw / twp,
							uvRatio.Y * th / thp
						);
						
						// Position
						float posX = tempPos.X + ((float)x - (float)halfMapW) * (float)TileW;
						float posY = tempPos.Y + ((float)y - (float)halfMapH) * (float)TileH;
						
						// Bottom-left
						vertices[0] = new VertexC1P3T2();
						vertices[0].Pos.X = (posX - tw / 2) * tempScale;
						vertices[0].Pos.Y = (posY + th / 2) * tempScale;
						vertices[0].Pos.Z = tempPos.Z;
						vertices[0].TexCoord.X = uvRect.LeftX;
						vertices[0].TexCoord.Y = uvRect.BottomY;
						vertices[0].Color = color;
						
						// Top-left
						vertices[1] = new VertexC1P3T2();
						vertices[1].Pos.X = (posX - tw / 2) * tempScale;
						vertices[1].Pos.Y = (posY - th / 2) * tempScale;
						vertices[1].Pos.Z = tempPos.Z;
						vertices[1].TexCoord.X = uvRect.LeftX;
						vertices[1].TexCoord.Y = uvRect.TopY;
						vertices[1].Color = color;
						
						// Top-right
						vertices[2] = new VertexC1P3T2();
						vertices[2].Pos.X = (posX + tw / 2) * tempScale;
						vertices[2].Pos.Y = (posY - th / 2) * tempScale;
						vertices[2].Pos.Z = tempPos.Z;
						vertices[2].TexCoord.X = uvRect.RightX;
						vertices[2].TexCoord.Y = uvRect.TopY;
						vertices[2].Color = color;
						
						// Bottom-right
						vertices[3] = new VertexC1P3T2();
						vertices[3].Pos.X = (posX + tw / 2) * tempScale;
						vertices[3].Pos.Y = (posY + th / 2) * tempScale;
						vertices[3].Pos.Z = tempPos.Z;
						vertices[3].TexCoord.X = uvRect.RightX;
						vertices[3].TexCoord.Y = uvRect.BottomY;
						vertices[3].Color = color;
						
						
		
						device.AddVertices(tileset.Image, VertexMode.Quads, vertices);
					}
				}
			}
		}
		
		public bool IsVisible( IDrawDevice device )
		{
			if( device.VisibilityMask == VisibilityFlag.AllGroups )
				return true;
			
			return false;
		}

		public float BoundRadius {
			get {
				return 1f;
			}
		}
	}
}
