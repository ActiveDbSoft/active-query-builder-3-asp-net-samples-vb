Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class ToggleUseAltNames
		Inherits BasePage
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("ToggleUseAltNames")

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
			Dim queryBuilder = QueryBuilderStore.Factory.DB2("ToggleUseAltNames")

			' Turn displaying of alternate names on in the text of result SQL query
			queryBuilder.SQLFormattingOptions.UseAltNames = False

			' Turn displaying of alternate names on in the visual UI
			queryBuilder.SQLGenerationOptions.UseAltNames = False
            
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

		Protected Sub OnCheckedChanged(sender As Object, e As EventArgs)
			Toggle()
		End Sub

		Public Sub Toggle()
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("ToggleUseAltNames")

			qb.SQLFormattingOptions.UseAltNames = Not qb.SQLFormattingOptions.UseAltNames
			qb.SQLGenerationOptions.UseAltNames = Not qb.SQLGenerationOptions.UseAltNames

			' Reload metadata structure to refill it with real or alternate names.
			' Note: reloading the structure does not reload the metadata container. 
			qb.MetadataStructure.Refresh()
		End Sub
	End Class
End Namespace
