Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server
Imports WebForms_Samples.Helpers

Namespace Samples
	Public Partial Class ChangeConnection
		Inherits BasePage
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("ChangeConnection")

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
			Dim qb = QueryBuilderStore.Create("ChangeConnection")

			SetNorthwindXml(qb)

			Return qb
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

		Protected Sub OnClick(sender As Object, e As EventArgs)
			Dim btn = DirectCast(sender, Button)
			Change(btn.Text)
		End Sub

		Public Sub Change(name As String)
			Dim queryBuilder = QueryBuilderStore.[Get]("ChangeConnection")

			queryBuilder.MetadataContainer.Clear()

			If name = "NorthwindXmlMetaData" Then
				SetNorthwindXml(queryBuilder)
			ElseIf name = "SQLite" Then
				SetSqLite(queryBuilder)
			Else
				SetDb2Xml(queryBuilder)
			End If
		End Sub
	End Class
End Namespace
