﻿// <file>
//	 <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//	 <version>$Revision: 2555 $</version>
// </file>

namespace NUnit.ConsoleRunner
{
	using System;
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Resources;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using NUnit.Core;
	using NUnit.Util;
	
	/// <summary>
	/// Custom event listener that writes out test results as they
	/// occur to a separate file.
	/// </summary>
	class EventCollector : MarshalByRefObject, EventListener
	{
		private int testRunCount;
		private int testIgnoreCount;
		private int failureCount;
		private int level;

		private ConsoleOptions options;
		private TextWriter outWriter;
		private TextWriter errorWriter;
		private TextWriter testResultWriter;

		StringCollection messages;
		
		private bool progress = false;
		private string currentTestName;
		private bool writeResults = false;

		private ArrayList unhandledExceptions = new ArrayList();

		public EventCollector( ConsoleOptions options, TextWriter outWriter, TextWriter errorWriter, TextWriter testResultWriter)
		{
			level = 0;
			this.options = options;
			this.outWriter = outWriter;
			this.errorWriter = errorWriter;
			this.currentTestName = string.Empty;
			this.progress = !options.xmlConsole && !options.labels && !options.nodots;
			this.testResultWriter = testResultWriter;
			this.writeResults = testResultWriter != null;

			AppDomain.CurrentDomain.UnhandledException +=
				new UnhandledExceptionEventHandler(OnUnhandledException);
		}

		public bool HasExceptions
		{
			get { return unhandledExceptions.Count > 0; }
		}

		public void WriteExceptions()
		{
			Console.WriteLine();
			Console.WriteLine("Unhandled exceptions:");
			int index = 1;
			foreach( string msg in unhandledExceptions )
				Console.WriteLine( "{0}) {1}", index++, msg );
		}

		public void RunStarted(string name, int testCount)
		{
		}

		public void RunFinished(TestResult result)
		{
		}

		public void RunFinished(Exception exception)
		{
		}

		public void TestFinished(TestCaseResult testResult)
		{
			if(testResult.Executed)
			{
				testRunCount++;

				if(testResult.IsFailure)
				{
					failureCount++;
					
					if ( progress )
						Console.Write("F");
					
					messages.Add( string.Format( "{0}) {1} :", failureCount, testResult.Test.TestName.FullName ) );
					messages.Add( testResult.Message.Trim( Environment.NewLine.ToCharArray() ) );

					string stackTrace = StackTraceFilter.Filter( testResult.StackTrace );
					if ( stackTrace != null && stackTrace != string.Empty )
					{
						string[] trace = stackTrace.Split( System.Environment.NewLine.ToCharArray() );
						foreach( string s in trace )
						{
							if ( s != string.Empty )
							{
								string link = Regex.Replace( s.Trim(), @".* in (.*):line (.*)", "$1($2)");
								messages.Add( string.Format( "at\n{0}", link ) );
							}
						}
					}
				}
			}
			else
			{
				testIgnoreCount++;
				
				if ( progress )
					Console.Write("N");
			}

			if (writeResults) {
				WriteTestResult(testResult);
			}

			currentTestName = string.Empty;
		}

		public void TestStarted(TestName testName)
		{
			currentTestName = testName.FullName;

			if ( options.labels )
				outWriter.WriteLine("***** {0}", currentTestName );
			
			if ( progress )
				Console.Write(".");
		}

		public void SuiteStarted(TestName testName)
		{
			if ( level++ == 0 )
			{
				messages = new StringCollection();
				testRunCount = 0;
				testIgnoreCount = 0;
				failureCount = 0;
				Trace.WriteLine( "################################ UNIT TESTS ################################" );
				Trace.WriteLine( "Running tests in '" + testName.FullName + "'..." );
			}
		}

		public void SuiteFinished(TestSuiteResult suiteResult)
		{
			if ( --level == 0)
			{
				Trace.WriteLine( "############################################################################" );

				if (messages.Count == 0)
				{
					Trace.WriteLine( "##############				 S U C C E S S			   #################" );
				}
				else
				{
					Trace.WriteLine( "##############				F A I L U R E S			  #################" );
					
					foreach ( string s in messages )
					{
						Trace.WriteLine(s);
					}
				}

				Trace.WriteLine( "############################################################################" );
				Trace.WriteLine( "Executed tests	   : " + testRunCount );
				Trace.WriteLine( "Ignored tests		: " + testIgnoreCount );
				Trace.WriteLine( "Failed tests		 : " + failureCount );
				Trace.WriteLine( "Unhandled exceptions : " + unhandledExceptions.Count);
				Trace.WriteLine( "Total time		   : " + suiteResult.Time + " seconds" );
				Trace.WriteLine( "############################################################################");
			}
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject.GetType() != typeof(System.Threading.ThreadAbortException))
			{
				this.UnhandledException((Exception)e.ExceptionObject);
			}
		}


		public void UnhandledException( Exception exception )
		{
			// If we do labels, we already have a newline
			unhandledExceptions.Add(currentTestName + " : " + exception.ToString());
			//if (!options.labels) outWriter.WriteLine();
			string msg = string.Format("##### Unhandled Exception while running {0}", currentTestName);
			//outWriter.WriteLine(msg);
			//outWriter.WriteLine(exception.ToString());

			Trace.WriteLine(msg);
			Trace.WriteLine(exception.ToString());
		}

		public void TestOutput( TestOutput output)
		{
			switch ( output.Type )
			{
				case TestOutputType.Out:
					outWriter.Write( output.Text );
					break;
				case TestOutputType.Error:
					errorWriter.Write( output.Text );
					break;
			}
		}


		public override object InitializeLifetimeService()
		{
			return null;
		}
		
		/// <summary>
		/// Writes a single test result to the output result file.
		/// </summary>
		void WriteTestResult(TestCaseResult testResult)
		{
			testResultWriter.WriteLine("Name: " + testResult.Name);
			
			if (testResult.IsFailure) 
			{
				testResultWriter.WriteLine("Result: Failure");
				testResultWriter.WriteLine("Message: " + EncodeText(testResult.Message));
				testResultWriter.WriteLine("StackTrace: " + EncodeText(StackTraceFilter.Filter(testResult.StackTrace)));
			} 
			else if (!testResult.Executed)
			{
				testResultWriter.WriteLine("Result: Ignored");
			}
			else if (testResult.IsSuccess)
			{
				testResultWriter.WriteLine("Result: Success");
			} 
		}

		/// <summary>
		/// Replaces the first character on each new line with a space.
		/// The first line does not have the extra space added.
		/// </summary>
		static string EncodeText(string s)
		{
			if (s == null || s.Length == 0) 
			{
				return String.Empty;
			}
			
			s = s.TrimEnd(Environment.NewLine.ToCharArray());
			
			StringBuilder encodedText = new StringBuilder();
			foreach (char ch in s) 
			{
				switch (ch)	
				{
					case '\n':
						encodedText.Append("\r\n ");
						break;
					case '\r':
						// Ignore.
						break;
					default:
						encodedText.Append(ch);
						break;
				}
			}
			return encodedText.ToString();
		}		
	}
}
