Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class ReactClientRenderingController
		Inherits Controller
		Public Function Index() As ActionResult
			CreateQueryBuilder()
			Return View()
		End Function

		''' <summary>
		''' Creates and initializes new instance of the QueryBuilder object if it doesn't exist. 
		''' </summary>
		Private Sub CreateQueryBuilder()
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("React")

			If qb IsNot Nothing Then
				Return
			End If

			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.MsSql("React")
            
			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name is stored in the "Web.config" file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			'Set default query
			queryBuilder.SQL = GetDefaultSql()
		End Sub

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                    c.CustomerID," & vbCr & vbLf & "                    s.ShipperID," & vbCr & vbLf & "                    o.ShipCity" & vbCr & vbLf & "                From Orders o" & vbCr & vbLf & "                    Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "                    Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "                Where o.ShipCity = 'A'"
		End Function
	End Class
End Namespace
