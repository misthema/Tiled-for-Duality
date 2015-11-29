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
	public class TiledLoader : Component, ICmpEditorUpdatable
	{
		public ContentRef<XmlData> TileDMap {get;set;}
		bool inited = false;
		
		public List<string> TilesetPaths {get;set;}
		
		public float BoundRadius {get;set;}
		
		
		public TiledMap Map {get;set;}
		
		public TiledLoader()
		{
			TilesetPaths = new List<string>();
			TilesetPaths.Add("Data\\");
		}
		
		public void OnUpdate()
		{
			if( !inited && Map == null && TileDMap != null )
			{
				LoadMap(TileDMap.Res);
				inited = true;
				
				/*if( Map != null )
					Log.Editor.Write("Tilemap loaded!");*/
			}
			else if( Map == null && inited)
			{
				inited = false;
				Map.DisposeLater();
				Map.Clear();
				//Map = null;
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
			PlayAround();
			
			Map = new TiledMap(map);
			Map.GameObj = this.GameObj;
			
			//printNode( map.XmlDocument.Root );
			
			
		}
		
		void PlayAround()
		{
			// Store integer 182
			int intValue = 16581375;
			// Convert integer 182 as a hex in a string variable
			string hexValue = intValue.ToString("X");
			// Convert the hex string back to the number
			int intAgain = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
			
			Log.Editor.Write("TEST: {0} = {1} = {2}", intValue, hexValue, intAgain);
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
