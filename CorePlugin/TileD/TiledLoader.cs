using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Duality;
using Duality.Drawing;
using Duality.Resources;
using Duality.Editor;

using SnowyPeak.Duality.Plugin.Data.Resources;
//using System.Xml.XPath;

namespace TileD_Plugin.TileD
{
	public class TiledLoader : Component, ICmpEditorUpdatable, ICmpRenderer
	{
		public ContentRef<XmlData> TileDMap {get;set;}
		public ContentRef<Texture> CustomTileset {get;set;}
		ContentRef<Texture> tileSet;
		bool inited = false;
		
		public List<string> TilesetPaths {get;set;}
		
		public float BoundRadius {get;set;}
		
		
		public TiledMap Map {get;set;}
		
		public TiledLoader()
		{
			TilesetPaths = new List<string>();
			TilesetPaths.Add("Data\\");
		}
		
		public void Draw(IDrawDevice device)
		{
			
			//tileSet.Res.LookupAtlas(			
		}
		
		public bool IsVisible(IDrawDevice device)
		{
			return true;
		}
		
		public void OnUpdate()
		{
			if( !inited && TileDMap != null)
			{
				Log.Editor.Write("Tilemap resource set...");
				LoadMap(TileDMap.Res);
				inited = true;
				
				/*if( Map != null )
					Log.Editor.Write("Tilemap loaded!");*/
			}
			else if( TileDMap == null && inited)
			{
				inited = false;
				Map = null;
			}
			
			
		}
		
		/*public static string Base64Encode(string plainText) {
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}*/
		
		/*public static string Base64Decode(string base64EncodedData) {
			var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
			return Encoding.UTF8.GetString(base64EncodedBytes);
		}*/
		
		public void LoadMap(XmlData map)
		{
			Map = new TiledMap(map);
			
			printNode( map.XmlDocument.Root );
			
			//PlayAround();
		}
		
		void PlayAround()
		{
			var content = ContentProvider.GetAvailableContent<Pixmap>();
			
			
			foreach( var con in content )
			{
				Log.Editor.Write("'{0}'", con.FullName);
				Log.Editor.Write("   '{0}'", con.Name);
			}
			
			
			uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
			uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
			uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;
			uint id = 1073747272;
			
			var obj = new TiledObject();
			obj.Gid = id;
			
			Log.Editor.Write("{0} --> {1}", id, obj.Gid);
			Log.Editor.Write("TiledFlipped.{0}", obj.Flipped);
			
			Log.Editor.Write("terrain.Split(',') = {0},{1},{2},{3}",
			               "0,0,,0".Split(',')[0],
			               "0,0,,0".Split(',')[1],
			               "0,0,,0".Split(',')[2],
			               "0,0,,0".Split(',')[3]
			              );
		}
		
		void printNode(XNode node, int depth = 0)
		{
			if( node == null ) return;
			
			string spaces = "";
			for( int i = 0; i < depth; i++ )
				spaces += "    ";
			
			var x = (XElement)node;
			
			Log.Editor.Write(spaces + "Node: {0}", x.Name);
			
			if( x.HasAttributes )
			{
				Log.Editor.Write(spaces + "Attributes: ");
				
				foreach( var attr in x.Attributes() )
				{
					Log.Editor.Write(spaces + "  {0} => \"{1}\"", attr.Name, attr.Value);
				}
			}
			
			if( x.HasElements )
			{
				Log.Editor.Write(spaces + "Elements: ");
				foreach( var ele in x.Elements() )
				{
					printNode(ele, depth + 1);
				}
			}
			
			if( x.Value != "" )
			{
				Log.Editor.Write(spaces + "Node value: ");
				Log.Editor.Write(spaces + "    {0}", x.Value);
			}
		}
		
		/*public static void DecompressGZip(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }*/
	}
}
