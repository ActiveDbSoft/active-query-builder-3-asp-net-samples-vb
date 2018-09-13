Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class AlternateNames
		Inherits BasePage
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("AlternateNames")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			QueryBuilderControl1.QueryBuilder = qb
			ObjectTreeView1.QueryBuilder = qb
			Canvas1.QueryBuilder = qb
			Grid1.QueryBuilder = qb
			SubQueryNavigationBar1.QueryBuilder = qb
			SqlEditor1.QueryBuilder = qb
			StatusBar1.QueryBuilder = qb
		End Sub

		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.DB2("AlternateNames")

			' Turn displaying of alternate names on in the text of result SQL query
			queryBuilder.SQLFormattingOptions.UseAltNames = True

			' Turn displaying of alternate names on in the visual UI
			queryBuilder.SQLGenerationOptions.UseAltNames = True

			Dim sq As SQLQuery = queryBuilder.SQLQuery
			AddHandler sq.SQLUpdated, AddressOf OnSQLUpdated
			queryBuilder.MetadataLoadingOptions.OfflineMode = True
            
			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("Db2XmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			'Set default query
			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "Select ""Employees"".""Employee ID"", ""Employees"".""First Name"", ""Employees"".""Last Name"", ""Employee Photos"".""Photo Image"", ""Employee Resumes"".Resume From ""Employee Photos"" Inner Join" & vbCr & vbLf & vbTab & vbTab & vbTab & """Employees"" On ""Employee Photos"".""Employee ID"" = ""Employees"".""Employee ID"" Inner Join" & vbCr & vbLf & vbTab & vbTab & vbTab & """Employee Resumes"" On ""Employee Resumes"".""Employee ID"" = ""Employees"".""Employee ID"""
		End Function

		Public Sub OnSQLUpdated(sender As Object, e As EventArgs)
			Dim qb = QueryBuilderStore.[Get]("AlternateNames")

			Dim opts = New SQLFormattingOptions()

			opts.Assign(qb.SQLFormattingOptions)
			opts.KeywordFormat = KeywordFormat.UpperCase

			' get SQL query with real object names
			opts.UseAltNames = False
			Dim plainSql = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts)

			' get SQL query with alternate names
			opts.UseAltNames = True
			Dim sqlWithAltNames = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts)

			' prepare additional data to be sent to the client
			qb.ExchangeData = New With { _
				Key .SQL = plainSql, _
				Key .AlternateSQL = sqlWithAltNames _
			}
		End Sub
	End Class
End Namespace
