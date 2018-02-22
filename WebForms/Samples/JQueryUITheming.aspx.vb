Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class JQueryUITheming
		Inherits Page
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("JQueryUiTheming")

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
			Dim queryBuilder = QueryBuilderStore.Create("JQueryUiTheming")

			' Create an instance of the proper syntax provider for your database server.
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			'Set default query
			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                        c.CustomerID," & vbCr & vbLf & "                        s.ShipperID," & vbCr & vbLf & "                        o.ShipCity" & vbCr & vbLf & "                    From Orders o" & vbCr & vbLf & "                        Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "                        Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "                    Where o.ShipCity = 'A'"
		End Function
	End Class
End Namespace
