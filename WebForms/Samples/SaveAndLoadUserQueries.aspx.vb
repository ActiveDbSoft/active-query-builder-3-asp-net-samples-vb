Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class SaveAndLoadUserQueries
		Inherits BasePage
	    ' //CUT:STD{{
	    Protected UserQueries1 As Global.ActiveQueryBuilder.Web.WebForms.UserQueries

		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("SaveAndLoadUserQueries")

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
			UserQueries1.QueryBuilder = qb
		End Sub

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

			' Subscribe to changing of the user queries container
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
			Dim file__1 = Server.MapPath("UserQueriesStructure.xml")

			If File.Exists(file__1) Then
				uqc.ImportFromXML(file__1)
			End If
		End Sub

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                        c.CustomerID," & vbCr & vbLf & "                        s.ShipperID," & vbCr & vbLf & "                        o.ShipCity" & vbCr & vbLf & "                    From Orders o" & vbCr & vbLf & "                        Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "                        Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "                    Where o.ShipCity = 'A'"
		End Function
	    ' //}}CUT:STD
	End Class
End Namespace
