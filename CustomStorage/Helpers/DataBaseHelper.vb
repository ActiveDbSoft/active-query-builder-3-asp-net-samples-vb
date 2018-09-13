Imports System.Configuration
Imports System.Data
Imports System.Data.SQLite
Imports System.IO
Imports System.Web

Namespace Helpers
	''' <summary>
	''' Helper methods to work with data and establish database connections.
	''' </summary>
	Public NotInheritable Class DataBaseHelper
		Private Sub New()
		End Sub
		''' <summary>
		''' Creates DBConnection object for SQLite database.
		''' </summary>
		''' <param name="AConfigurationName">Name of database configuration stored in the Web.Config file.</param>
		''' <returns>Returns an instance of SQLiteConnection.</returns>
		Public Shared Function CreateSqLiteConnection(AConfigurationName As String) As IDbConnection
			' File name stored in the "/configuration/appSettings/<configuration name>" key
			Dim path__1 = ConfigurationManager.AppSettings(AConfigurationName)
			Dim file = Path.Combine(HttpContext.Current.Server.MapPath("~"), path__1)

			Dim connectionString As String = String.Format("Data Source={0};Version=3;", file)

			Return New SQLiteConnection(connectionString)
		End Function
	End Class
End Namespace
