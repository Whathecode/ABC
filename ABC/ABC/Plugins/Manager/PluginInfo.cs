using System;
using System.Reflection;
using System.Runtime.InteropServices;


namespace ABC.Plugins.Manager
{
	/// <summary>
	///   Object containing plugin information.
	/// </summary>
	/// <remarks>
	///   This class was documented as introduced since cross-domain communication requires objects to be serializable.
	///   TODO: Isn't the original <see cref="Assembly" /> serializable?
	/// </remarks>
	[Serializable]
	public class PluginInfo 
	{
		readonly Assembly _assembly;

		/// <summary>
		///   Gets assembly title information.
		/// </summary>
		public string ProductTitle
		{
			get { return GetAttributeValue<AssemblyTitleAttribute, string>( a => a.Title ); }
		}

		/// <summary>
		///   Gets assembly version.
		/// </summary>
		public Version Version
		{
			get { return _assembly.GetName().Version; }
		}

		/// <summary>
		///   Gets assembly description information.
		/// </summary>
		public string Description
		{
			get { return GetAttributeValue<AssemblyDescriptionAttribute, string>( a => a.Description ); }
		}

		/// <summary>
		///  Gets product name information.
		/// </summary>
		public string Product
		{
			get { return GetAttributeValue<AssemblyProductAttribute, string>( a => a.Product ); }
		}

		/// <summary>
		///   Gets copyright information.
		/// </summary>
		public string Copyright
		{
			get { return GetAttributeValue<AssemblyCopyrightAttribute, string>( a => a.Copyright ); }
		}

		/// <summary>
		///   Gets the company name information.
		/// </summary>
		public string Company
		{
			get { return GetAttributeValue<AssemblyCompanyAttribute, string>( a => a.Company ); }
		}

		/// <summary>
		///   Gets the GUID.
		/// </summary>
		public Guid Guid
		{
			get { return GetAttributeValue<GuidAttribute, Guid>( a => new Guid( a.Value ) ); }
		}


		public PluginInfo( Assembly assembly )
		{
			if ( assembly == null )
			{
				throw new ArgumentNullException( nameof( assembly ) );
			}
			_assembly = assembly;
		}


		protected TSelector GetAttributeValue<TAttribute, TSelector>( Func<TAttribute, TSelector> resolveFunc )
			where TAttribute : Attribute
		{
			object[] attributes = _assembly.GetCustomAttributes( typeof( TAttribute ), false );
			return attributes.Length > 0 ? resolveFunc( (TAttribute)attributes[ 0 ] ) : default( TSelector );
		}
	}
}