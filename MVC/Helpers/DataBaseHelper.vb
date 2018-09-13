Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.OleDb
Imports System.IO
Imports System.Web
Imports System.Data.SQLite
Imports MVC_Samples.Controllers

Namespace Helpers
	''' <summary>
	''' Helper methods to work with data and establish database connections.
	''' </summary>
	Public NotInheritable Class DataBaseHelper
		Private Sub New()
		End Sub
		''' <summary>
		''' Executes a query and returns the result data as a list of records. Each record is represented as a list of field name-value pairs.
		''' </summary>
		''' <param name="conn">The DB connection object.</param>
		''' <param name="sql">The SQL query text.</param>
		''' <returns>List of records.</returns>
		Public Shared Function GetData(conn As IDbConnection, sql As String, parameters As Param()) As List(Of Dictionary(Of String, Object))
			Dim cmd As IDbCommand = conn.CreateCommand()
			cmd.CommandText = sql

			If String.IsNullOrEmpty(sql) Then
				Return New List(Of Dictionary(Of String, Object))()
			End If

			If parameters IsNot Nothing Then
				AddParameters(cmd, parameters)
			End If

			Try
				If conn.State <> ConnectionState.Open Then
					conn.Open()
				End If

				Dim reader As IDataReader = cmd.ExecuteReader()
				Return ConvertToList(reader)
			Finally
				conn.Close()
			End Try
		End Function

		Private Shared Sub AddParameters(cmd As IDbCommand, parameters As Param())
			For Each p As Param In parameters
				Dim param = cmd.CreateParameter()
				param.DbType = p.DataType
				param.ParameterName = p.Name
				param.Value = p.Value

				cmd.Parameters.Add(param)
			Next
		End Sub

		''' <summary>
		''' Saves data from IDataReader to the list. Each record is represented as a list of field name-value pairs.
		''' </summary>
		''' <param name="reader">The Data Reader object.</param>
		''' <returns>List of records.</returns>
		Private Shared Function ConvertToList(reader As IDataReader) As List(Of Dictionary(Of String, Object))
			Dim result = New List(Of Dictionary(Of String, Object))()

			While reader.Read()
				Dim row = New Dictionary(Of String, Object)()

				For i As Integer = 0 To reader.FieldCount - 1
					row.Add(reader.GetName(i), reader(i))
				Next

				result.Add(row)
			End While

			Return result
		End Function

		''' <summary>
		''' Executes a query and returns the result data as a list of values lists. Each record is represented as a list of values.
		''' </summary>
		''' <param name="conn">The DB connection object.</param>
		''' <param name="sql">The SQL query text.</param>
		''' <returns>List of values lists.</returns>
		Public Shared Function GetDataList(conn As IDbConnection, sql As String) As List(Of List(Of Object))
			Dim cmd As IDbCommand = conn.CreateCommand()
			cmd.CommandText = sql

			If String.IsNullOrEmpty(sql) Then
				Return New List(Of List(Of Object))()
			End If

			Try
				If conn.State <> ConnectionState.Open Then
					conn.Open()
				End If

				Dim reader As IDataReader = cmd.ExecuteReader()
				Return Convert(reader)
			Finally
				conn.Close()
			End Try
		End Function

		''' <summary>
		''' Saves data from IDataReader to the list. Each record is represented as a list of values.
		''' </summary>
		''' <param name="reader">The Data Reader object.</param>
		''' <returns>List of values lists.</returns>
		Private Shared Function Convert(reader As IDataReader) As List(Of List(Of Object))
			Dim result = New List(Of List(Of Object))()

			While reader.Read()
				Dim row = New List(Of Object)()

				For i As Integer = 0 To reader.FieldCount - 1
					row.Add(reader(i))
				Next

				result.Add(row)
			End While

			Return result
		End Function

		''' <summary>
		''' Creates DBConnection object for MS Access database.
		''' </summary>
		''' <param name="AConfigurationName">Name of database configuration stored in the Web.Config file.</param>
		''' <returns>Returns an instance of OLEDBConnection.</returns>
		Public Shared Function CreateMSAccessConnection(AConfigurationName As String) As IDbConnection
			'var provider = "Microsoft.ACE.OLEDB.12.0";
			Dim provider = "Microsoft.Jet.OLEDB.4.0"

			' File name stored in the "/configuration/appSettings/<configuration name>" key
			Dim path__1 = ConfigurationManager.AppSettings(AConfigurationName)
			Dim file = Path.Combine(HttpContext.Current.Server.MapPath("~"), path__1)
			Dim connectionString = String.Format("Provider={0};Data Source={1};Persist Security Info=False;", provider, file)

			Return New OleDbConnection(connectionString)
		End Function

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
