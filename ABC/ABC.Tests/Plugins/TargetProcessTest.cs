using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whathecode.Microsoft.VisualStudio.TestTools.UnitTesting;


namespace ABC.Plugins
{
	[TestClass]
	public class TargetProcessTest
	{
		[TestMethod]
		public void ConstructorTest()
		{
			const string processName = "explorer";
			const string companyName = "Microsoft Corporation";

			// Test version parameter.
			string[] shouldPass = { "1.0", "1.1.1", "3.5.7.5", null };
			string[] shouldFail = { "jshdf", "1.*", "4.5.6.7.5" };
			foreach ( string pass in shouldPass )
			{
				// ReSharper disable once ObjectCreationAsStatement
				new TargetProcess( processName, companyName, pass );
			}
			foreach ( string fail in shouldFail )
			{
				// ReSharper disable once ObjectCreationAsStatement
				AssertHelper.ThrowsException<ArgumentException>( () => new TargetProcess( processName, companyName, fail ) );
			}
		}
	}
}