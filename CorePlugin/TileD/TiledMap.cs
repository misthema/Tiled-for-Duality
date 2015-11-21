using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Duality;
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
	[EditorHintCategory(CoreResNames.CategoryPhysics)]
	[EditorHintImage(CoreResNames.ImageRigidBody)]
	public class TiledMap : Component, ICmpEditorUpdatable
	{
		public void OnUpdate()
		{
			
		}
		
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
		
		
		public TiledMap(XmlData map)
		{
			//this.GameObj = obj;
			
			var root = map.XmlDocument.Root;
			XElement node;
			
			
			
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
									
									break;
								case "tileset":
									TiledTileset tileset = LoadTileset(node);
									
									break;
								case "objectgroup":
									TiledObjectGroup objGroup = LoadObjectGroup(node);
									
									break;
								case "imagelayer":
									TiledImageLayer imgLayer = LoadImageLayer(node);
									
									break;
							}
						}
						
						break;
					
				}
				
				Log.Editor.Write("Width: {0}, Height: {1}", W, H);
			}
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
								break;
								
							case "image":
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
		
	}
}
