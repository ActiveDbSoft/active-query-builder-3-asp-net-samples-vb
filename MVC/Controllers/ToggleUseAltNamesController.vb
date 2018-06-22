Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class ToggleUseAltNamesController
		Inherits Controller
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("ToggleUseAltNames")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Public Function Toggle() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("ToggleUseAltNames")

			qb.SQLFormattingOptions.UseAltNames = Not qb.SQLFormattingOptions.UseAltNames
			qb.SQLGenerationOptions.UseAltNames = Not qb.SQLGenerationOptions.UseAltNames

			' Reload metadata structure to refill it with real or alternate names.
			' Note: reloading the structure does not reload the metadata container. 
			qb.MetadataStructure.Refresh()

			Return New EmptyResult()
		End Function

		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.DB2("ToggleUseAltNames")

			queryBuilder.SQLFormattingOptions.UseAltNames = False
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
	End Class
End Namespace
