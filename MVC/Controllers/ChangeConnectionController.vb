Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server
Imports MVC_Samples.Helpers

Namespace Controllers
	Public Class ChangeConnectionController
		Inherits Controller
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("ChangeConnection")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.Create("ChangeConnection")

			SetNorthwindXml(qb)

			Return qb
		End Function

		<HttpPost> _
		Public Function Change(name As String) As ActionResult
			Dim queryBuilder = QueryBuilderStore.[Get]("ChangeConnection")

			queryBuilder.MetadataContainer.Clear()

			If name = "NorthwindXmlMetaData" Then
				SetNorthwindXml(queryBuilder)
			ElseIf name = "SQLite" Then
				SetSqLite(queryBuilder)
			Else
				SetDb2Xml(queryBuilder)
			End If

			Return New EmptyResult()
		End Function

		Private Sub SetNorthwindXml(qb As QueryBuilder)
			qb.MetadataLoadingOptions.OfflineMode = True
			qb.SyntaxProvider = New MSSQLSyntaxProvider()

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			qb.MetadataContainer.ImportFromXML(xml)
			qb.MetadataStructure.Refresh()
		End Sub

		Private Sub SetDb2Xml(qb As QueryBuilder)
			qb.MetadataLoadingOptions.OfflineMode = True
			qb.SyntaxProvider = New DB2SyntaxProvider()

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("Db2XmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			qb.MetadataContainer.ImportFromXML(xml)
			qb.MetadataStructure.Refresh()
		End Sub

		Private Sub SetSqLite(qb As QueryBuilder)
			qb.MetadataLoadingOptions.OfflineMode = False
			qb.SyntaxProvider = New SQLiteSyntaxProvider()
			qb.MetadataProvider = New SQLiteMetadataProvider() With { _
				.Connection = DataBaseHelper.CreateSqLiteConnection("SqLiteDataBase") _
			}

			qb.MetadataStructure.Refresh()
		End Sub
	End Class
End Namespace
