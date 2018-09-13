Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class SaveAndLoadUserQueriesController
		Inherits Controller
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("SaveAndLoadUserQueries")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.MsSql("SaveAndLoadUserQueries")
            
			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			ImportUserQueries(queryBuilder.UserQueries)
			Dim uq As UserQueriesContainer = queryBuilder.UserQueries
			AddHandler uq.Changed, AddressOf UserQueriesChanged

			'Set default query
			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Sub UserQueriesChanged(sender As Object, item As MetadataStructureItem)
			Dim container = DirectCast(sender, UserQueriesContainer)
			container.ExportToXML(Server.MapPath("UserQueriesStructure.xml"))
		End Sub

		Private Sub ImportUserQueries(uqc As UserQueriesContainer)
			Dim file = Server.MapPath("UserQueriesStructure.xml")

			If System.IO.File.Exists(file) Then
				uqc.ImportFromXML(file)
			End If
		End Sub

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                        c.CustomerID," & vbCr & vbLf & "                        s.ShipperID," & vbCr & vbLf & "                        o.ShipCity" & vbCr & vbLf & "                    From Orders o" & vbCr & vbLf & "                        Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "                        Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "                    Where o.ShipCity = 'A'"
		End Function
	End Class
End Namespace
