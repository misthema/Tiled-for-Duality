using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Duality;
using Duality.Drawing;
using Duality.IO;
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
	
	public static class TiledHelper
	{
		public static string GetAttribute(XElement node, string name)
		{
			if( node != null )
			{
				if( !string.IsNullOrEmpty(name) )
				{
					if( node.HasAttributes )
					{
						if( node.Attribute(name) != null)
						{
							if( !string.IsNullOrEmpty(node.Attribute(name).Value) )
								return node.Attribute(name).Value;
							
							Log.Editor.WriteWarning("TiledHelper.GetAttribute() :: Node '{0}', attribute '{1}' has no value.", node.Name, name);
						}
						else
						{
							Log.Editor.WriteWarning("TiledHelper.GetAttribute() :: Node '{0}' has no attribute '{1}'.", node.Name, name);
						}
					}
					else
					{
						Log.Editor.WriteWarning("TiledHelper.GetAttribute() :: Node '{0}' has no attributes.", node.Name);
					}
				}
				else
				{
					Log.Editor.WriteWarning("TiledHelper.GetAttribute() :: No attribute name specified.");
				}
			}
			else
			{
				Log.Editor.WriteWarning("TiledHelper.GetAttribute() :: Node is NULL.");
			}

			return string.Empty;
		}
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
							
							Log.Editor.Write("Found <{0}>...", node.Name);
							
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
			}
			
			Log.Editor.Write("Width: {0}, Height: {1}", W, H);
			Log.Editor.Write("Layer order:");
			foreach( var la in Layers.Values )
			{
				Log.Editor.Write("    {0}", la.Name );
			}
			
			Log.Editor.Write(ToString());
			
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
			Version = TiledHelper.GetAttribute(node, "version"); //node.Attribute("version").Value;
			Orientation = TiledHelper.GetAttribute(node, "orientation"); //node.Attribute("orientation").Value;
			NextObjectID = int.Parse(TiledHelper.GetAttribute(node, "nextobjectid")); //node.Attribute("nextobjectid").Value);
			
			switch( TiledHelper.GetAttribute(node, "renderorder") ) //node.Attribute("renderorder").Value )
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
			W = int.Parse(TiledHelper.GetAttribute(node, "width")); //node.Attribute("width").Value);
			H = int.Parse(TiledHelper.GetAttribute(node, "height")); //node.Attribute("height").Value);
			
			// Tile sizes (in pixels)
			TileW = int.Parse(TiledHelper.GetAttribute(node, "tilewidth")); //node.Attribute("tilewidth").Value);
			TileH = int.Parse(TiledHelper.GetAttribute(node, "tileheight")); //node.Attribute("tileheight").Value);
		}
		
		TiledLayer LoadLayer( XElement node )
		{
			if( node.HasAttributes )
			{
				Log.Editor.Write("Loading layer '{0}'...", node.Attribute("name").Value);
				
				var layer = new TiledLayer();
				layer.Parent = this;
				
				foreach( var attribute in node.Attributes() )
				{
					Log.Editor.Write("    {0} = \"{1}\"", attribute.Name, attribute.Value );
					
					switch( attribute.Name.LocalName )
					{
						case "name":
							layer.Name = attribute.Value;
							break;
							
						case "width":
							layer.W = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
							break;
							
						case "height":
							layer.H = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
							break;
							
						case "opacity":
							layer.Opacity = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						case "visible":
							layer.Visible = false;
							break;
							
						default:
							Log.Editor.WriteWarning("Attribute {0} not supported in layers.", attribute.Name);
							break;
					}
				}
				
				foreach( var element in node.Elements() )
				{
					switch( element.Name.LocalName )
					{
						case "properties":
							layer.LoadProperties( element );
							break;
							
						case "data":
							layer.LoadTiles( element );
							break;
							
						default:
							Log.Editor.WriteWarning("Element {0} not supported in layers.", element.Name);
							break;
					}
				}
				
				return layer;
			}
			
			Log.Editor.WriteWarning("Empty layer with no data, ignoring...");
			return null;
		}
		
		TiledTileset LoadTileset( XElement node )
		{
			if( node.HasAttributes )
			{
				Log.Editor.Write("Loading tileset..."); //node.Attribute("name").Value);
				
				var tileset = new TiledTileset();
				tileset.Parent = this;
				
				return LoadTilesetData( node, tileset );
			}
			
			return null;
		}
		
		TiledTileset LoadTilesetData( XElement node, TiledTileset tileset )
		{
			if( node.Name != "tileset" )
			{
				Log.Editor.WriteError("Cannot load tileset data! (Element should be <tileset>, not <{0}>)", node.Name);
				return null;
			}
			
			if( tileset == null )
			{
				Log.Editor.WriteError("Cannot load tileset data! (Tileset is NULL)");
				return null;
			}
			
			// Read attributes
			foreach( var attribute in node.Attributes() )
			{
				switch( attribute.Name.LocalName )
				{
					case "firstgid":
						tileset.FirstGID = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
						break;
						
					case "name":
						tileset.Name = attribute.Value;
						break;
						
					case "tilewidth":
						tileset.TileW = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
						break;
						
					case "tileheight":
						tileset.TileH = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
						break;
						
					case "tilecount":
						tileset.TileCount = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
						break;
						
					// Tilemap is using an external tileset file; go recursive!
					case "source":
						using( TextReader txt = new StreamReader(FileOp.Open(@"Source\Media\"+attribute.Value, FileAccessMode.Read) ) )
						{
							var xml = new XmlData();
							var data = txt.ReadToEnd();
							xml.SetData(data, data.Length, Encoding.Unicode);
							xml.Validate(); // Not sure if this is necessary
							
							// Data is valid
							if( xml.IsValid )
								return LoadTilesetData(xml.XmlDocument.Root, tileset);
							
							Log.Editor.WriteError("Unable to load external tileset!");
							return null;
						}
						
					default:
						Log.Editor.WriteWarning("Attribute {0} not supported in tilesets.", attribute.Name);
						break;
				}
			}

			foreach( var xelem in node.Elements() )
			{
				var element = (XElement)xelem;
				
				switch( element.Name.LocalName )
				{
					case "properties":
						tileset.LoadProperties( element );
						break;
						
					case "tileoffset":
						tileset.LoadOffset( element );
						break;
						
					case "image":
						tileset.LoadImage( element );
						break;
						
					case "terraintypes":
						tileset.LoadTerrainTypes( element );
						break;
						
					case "tile":
						tileset.LoadTerrainTile( element );
						break;
						
					default:
						Log.Editor.WriteWarning("Element {0} not supported in tilesets.", element.Name);
						break;
				}
			}
			
			return tileset;
		}
		
		TiledObjectGroup LoadObjectGroup( XElement node )
		{
			if( node.HasAttributes )
			{
				Log.Editor.Write("Loading object group '{0}'...", TiledHelper.GetAttribute(node, "name"));
				
				var objGroup = new TiledObjectGroup();
				objGroup.Parent = this;
				
				foreach( var attribute in node.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "color":
							objGroup.Color = new ColorRgba( int.Parse(attribute.Value.Replace("#", ""), System.Globalization.NumberStyles.HexNumber) );
							break;
							
						case "name":
							objGroup.Name = attribute.Value;
							break;
							
						case "opacity":
							objGroup.Opacity = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
						
						default:
							Log.Editor.WriteWarning("Attribute {0} not supported in object groups.", attribute.Name);
							break;
					}
				}
				
				foreach( var element in node.Elements() )
				{
					switch( element.Name.LocalName )
					{
						case "properties":
							objGroup.Properties.Extend( element );
							break;
							
						case "object":
							objGroup.LoadObject( element );
							break;
							
						default:
							Log.Editor.WriteWarning("Element {0} not supported in object groups.", element.Name);
							break;
					}
				}
				
				
				return objGroup;
			}
			
			
			//objGroup.Parent = this;
			
			return null;
		}
		
		TiledImageLayer LoadImageLayer( XElement node )
		{
			if( node.HasAttributes )
			{
				Log.Editor.Write("Loading image layer '{0}'...", node.Attribute("name").Value);
				
				var imgLayer = new TiledImageLayer();
				imgLayer.Parent = this;
				
				foreach( var attribute in node.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "name":
							imgLayer.Name = attribute.Value;
							break;
							
						case "x":
							imgLayer.X = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
							break;
							
						case "y":
							imgLayer.Y = int.Parse(attribute.Value, System.Globalization.NumberStyles.Integer);
							break;
							
						case "opacity":
							imgLayer.Opacity = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						default:
							Log.Editor.WriteWarning("Attribute {0} not supported in image layers.", attribute.Name);
							break;
					}
				}
				
				foreach( var element in node.Elements() )
				{
					switch( element.Name.LocalName )
					{
						case "image":
							imgLayer.LoadImage( element );
							break;
							
						case "properties":
							imgLayer.LoadProperties( element );
							break;
							
						default:
							Log.Editor.WriteWarning("Element {0} not supported in image layers.", element.Name);
							break;
					}
				}
				
				return imgLayer;
			}
				
			
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
						
						// Get tileset main color and set layer's opacity on it
						var color = tileset.Image.Res.MainColor.WithAlpha( layer.Opacity );
						
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
		
		public override string ToString()
		{
			string rtnString;
			
			rtnString = string.Format("[TiledMap RenderOrder={0}, Pos={1}, W={2}, H={3}, TileW={4}, TileH={5}, NextObjectID={6}, Version={7}, Orientation={8}, Tilesets={9}, Layers={10}, ObjectGroups={11}, ImageLayers={12}]", RenderOrder, Pos, W, H, TileW, TileH, NextObjectID, Version, Orientation, Tilesets, Layers, ObjectGroups, ImageLayers);
			
			return rtnString;
		}

	}
}
