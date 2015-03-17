using System;
using System.Reflection;
using System.Runtime.InteropServices;


namespace ABC.Plugins
{
	public class AssemblyInfo 
	{
		public AssemblyInfo( Assembly assembly )
		{
			if ( assembly == null )
				throw new ArgumentNullException( "assembly" );
			_assembly = assembly;
		}

		readonly Assembly _assembly;

		/// <summary>
		/// Gets the title property
		/// </summary>
		public string ProductTitle
		{
			get { return GetAttributeValue<AssemblyTitleAttribute, string>( a => a.Title ); }
		}

		/// <summary>
		/// Gets the application's version
		/// </summary>
		public Version Version
		{
			get { return _assembly.GetName().Version; }
		}

		/// <summary>
		/// Gets the description about the application.
		/// </summary>
		public string Description
		{
			get { return GetAttributeValue<AssemblyDescriptionAttribute, string>( a => a.Description ); }
		}


		/// <summary>
		///  Gets the product's full name.
		/// </summary>
		public string Product
		{
			get { return GetAttributeValue<AssemblyProductAttribute, string>( a => a.Product ); }
		}

		/// <summary>
		/// Gets the copyright information for the product.
		/// </summary>
		public string Copyright
		{
			get { return GetAttributeValue<AssemblyCopyrightAttribute, string>( a => a.Copyright ); }
		}

		/// <summary>
		/// Gets the company information for the product.
		/// </summary>
		public string Company
		{
			get { return GetAttributeValue<AssemblyCompanyAttribute, string>( a => a.Company ); }
		}

		/// <summary>
		/// Gets the GUID for the product.
		/// </summary>
		public Guid Guid
		{
			get { return GetAttributeValue<GuidAttribute, Guid>( a => new Guid( a.Value ) ); }
		}

		/// <summary>
		/// Version of product's target process.
		/// </summary>
		public Version TargetProcessVersion
		{
			get { return GetAttributeValue<AssemblyTargetProcess, Version>( a => a.Version ); }
		}

		/// <summary>
		///   The name of product's target process.
		/// </summary>
		public string TargetProcessName
		{
			get { return GetAttributeValue<AssemblyTargetProcess, string>( a => a.Name ); }
		}

		/// <summary>
		///   The company name if product's target process.
		/// </summary>
		public string TargetProcessCompanyName
		{
			get { return GetAttributeValue<AssemblyTargetProcess, string>( a => a.CompanyName ); }
		}

		protected T GetAttributeValue<TAttr, T>( Func<TAttr,
			T> resolveFunc ) where TAttr : Attribute
		{
			object[] attributes = _assembly.GetCustomAttributes( typeof( TAttr ), false );
			return attributes.Length > 0 ? resolveFunc( (TAttr)attributes[ 0 ] ) : default( T );
		}
	}
}