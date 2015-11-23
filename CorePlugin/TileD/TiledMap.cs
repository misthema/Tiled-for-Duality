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
	public class TiledMap : Component, ICmpRenderer
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
			}
			
			_loaded = true;
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
			Log.Editor.Write("Loading object group '{0}'...", node.Attribute("name").Value);
			
			//objGroup.Parent = this;
			
			return null;
		}
		
		TiledImageLayer LoadImageLayer( XElement node )
		{
			Log.Editor.Write("Loading image layer '{0}'...", node.Attribute("name").Value);
			
			//imgLayer.Parent = this;
			
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
				
				var vertices = new VertexC1P3T2[4];
				
				for (int y = 0; y < H; y++) {
					
					for (int x = 0; x < W; x++) {
						
						// Is renderable tile available?
						int gid = layer.GetTile(x, y);
						if (gid <= 0)
							continue;
						
						// Get the correct tileset for this GID
						var tileset = TiledTileset.FindByGID(gid);
						if (tileset == null)
							continue;
						
						
						
						int tileX = (gid - tileset.FirstGID) % tileset.W;
						int tileY = (gid - tileset.FirstGID) / tileset.W;
						
						var uvRect = new Rect(
							(tileX * TileW) / tileset.WPixel,
							(tileY * TileH) / tileset.HPixel,
							(tileX * TileW + TileW) / tileset.WPixel,
							(tileY * TileH + TileH) / tileset.HPixel
						);
						
						int posX = (int)(tempPos.X) + (x - halfMapW) * TileW;
						int posY = (int)(tempPos.Y) + (y - halfMapH) * TileH;
						
						// Top-left
						vertices[0] = new VertexC1P3T2();
						vertices[0].Pos.X = posX - (TileW / 2);
						vertices[0].Pos.Y = posY - (TileH / 2);
						vertices[0].Pos.Z = tempPos.Z;
						vertices[0].TexCoord.X = uvRect.X;
						vertices[0].TexCoord.Y = uvRect.Y;
						vertices[0].Color = ColorRgba.White;
						
						// Bottom-left
						vertices[1] = new VertexC1P3T2();
						vertices[1].Pos.X = posX - (TileW / 2);
						vertices[1].Pos.Y = posY + (TileH / 2);
						vertices[1].Pos.Z = tempPos.Z;
						vertices[1].TexCoord.X = uvRect.X;
						vertices[1].TexCoord.Y = uvRect.BottomY;
						vertices[1].Color = ColorRgba.White;
						
						// Bottom-right
						vertices[2] = new VertexC1P3T2();
						vertices[2].Pos.X = posX + (TileW / 2);
						vertices[2].Pos.Y = posY + (TileH / 2);
						vertices[2].Pos.Z = tempPos.Z;
						vertices[2].TexCoord.X = uvRect.RightX;
						vertices[2].TexCoord.Y = uvRect.BottomY;
						vertices[2].Color = ColorRgba.White;
						
						// Top-right
						vertices[3] = new VertexC1P3T2();
						vertices[3].Pos.X = posX + (TileW / 2);
						vertices[3].Pos.Y = posY - (TileH / 2);
						vertices[3].Pos.Z = tempPos.Z;
						vertices[3].TexCoord.X = uvRect.RightX;
						vertices[3].TexCoord.Y = uvRect.Y;
						vertices[3].Color = ColorRgba.White;
						
						device.AddVertices(tileset.Image, VertexMode.Quads, vertices);
					}
				}
			}
		}
		
		public bool IsVisible( IDrawDevice device )
		{
			return true;
		}

		public float BoundRadius {
			get {
				return 1f;
			}
		}
	}
}
