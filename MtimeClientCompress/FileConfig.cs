using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace MtimeClientCompress.Components
{
	#region CombinationFileInfo
	public class CombinationFileInfo
	{
		private string filename = string.Empty;
		private List<string> includeFilenames = new List<string> ();

		public string Filename
		{
			get
			{
				return filename;
			}
			set
			{
				filename = value;
			}
		}

		public List<string> IncludeFilenames
		{
			get
			{
				return includeFilenames;
			}
			set
			{
				includeFilenames = value;
			}
		}

		public void Add( string includeFilename )
		{
			includeFilenames.Add ( includeFilename );
		}

		public CombinationFileInfo( string filename )
		{
			this.filename = filename;
		}
	} 
	
	#endregion

	public sealed class FileConfig
	{
		#region Member variables & constructor
		private const string configFile = "Configs.xml";
		static readonly object lockObject = new object ();
		private static FileConfig instance = null;
		private readonly List<CombinationFileInfo> configs = null;

		public FileConfig( XmlDocument doc )
		{
			configs = new List<CombinationFileInfo> ();
			XmlNode root = doc.SelectSingleNode ( "Files" );
			foreach ( XmlNode fileNode in root.ChildNodes )
			{
				if ( fileNode.NodeType != XmlNodeType.Comment )
				{
					string name = fileNode.Attributes ["Name"].Value.ToLower ();
					CombinationFileInfo combinationFile = new CombinationFileInfo ( name );
					foreach ( XmlNode includeFileNode in fileNode.ChildNodes )
					{
						if ( includeFileNode.NodeType != XmlNodeType.Comment )
						{
							string includeFilename = includeFileNode.Attributes ["Name"].Value.ToLower ();
							combinationFile.Add ( includeFilename );
						}
					}
					configs.Add ( combinationFile );
				}

			}
		}

		#endregion

		#region GetConfig
		public static FileConfig GetConfig()
		{
			if ( instance == null )
			{
				lock ( lockObject )
				{
					if ( instance == null )
					{
						string file = Path.Combine ( AppDomain.CurrentDomain.BaseDirectory, configFile );
						XmlDocument doc = new XmlDocument ();
						doc.Load ( file );
						instance = new FileConfig ( doc );
					}
				}
			}
			return instance;
		}

		#endregion

		#region Public Methods

		public List<CombinationFileInfo> GetFiles()
		{
			return configs;
		}

		#endregion

		#region Helper


		#endregion
	}
}
