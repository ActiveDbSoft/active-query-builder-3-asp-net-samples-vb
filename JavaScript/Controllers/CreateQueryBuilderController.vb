Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class CreateQueryBuilderController
		Inherits Controller
		Public Function Index() As ActionResult
			Return View()
		End Function

		''' <summary>
		''' Creates and initializes new instance of the QueryBuilder object for the given identifier if it doesn't exist. 
		''' </summary>
		''' <param name="name">Instance identifier of object in the current session.</param>
		''' <returns></returns>
		Public Function CreateQueryBuilder(name As String) As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get](name)

			If qb IsNot Nothing Then
				Return New EmptyResult()
			End If

			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Create(name)

			' Create an instance of the proper syntax provider for your database server.
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

			' Denies metadata loading requests from live database connection
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name is stored in the "Web.config" file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			'Set default query
			queryBuilder.SQL = GetDefaultSql()

			Return New EmptyResult()
		End Function

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                        c.CustomerID," & vbCr & vbLf & "                        s.ShipperID," & vbCr & vbLf & "                        o.ShipCity" & vbCr & vbLf & "                    From Orders o" & vbCr & vbLf & "                        Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "                        Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "                    Where o.ShipCity = 'A'"
		End Function
	End Class
End Namespace
