Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class SimpleOfflineDemo
		Inherits Page
		Const qbId As String = "Offline"
		' identifies instance of the QueryBuilder object within a session
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get](qbId)

			If qb Is Nothing Then
				qb = CreateQueryBuilder(qbId)
			End If

			QueryBuilderControl1.QueryBuilder = qb
			ObjectTreeView1.QueryBuilder = qb
			Canvas1.QueryBuilder = qb
			Grid1.QueryBuilder = qb
			SubQueryNavigationBar1.QueryBuilder = qb
			SqlEditor1.QueryBuilder = qb
			StatusBar1.QueryBuilder = qb
		End Sub

		''' <summary>
		''' Creates and initializes a new instance of the QueryBuilder object.
		''' </summary>
		''' <param name="AInstanceId">String which uniquely identifies an instance of Active Query Builder in the session.</param>
		''' <returns>Returns instance of the QueryBuilder object.</returns>
		Private Function CreateQueryBuilder(AInstanceId As String) As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Create(AInstanceId)

			' Create an instance of the proper syntax provider for your database server.
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

			' Denies metadata loading requests from live database connection
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load metadata from XML document. File name stored in the "Web.config" file [/configuration/appSettings/NorthwindXmlMetaData] key
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
