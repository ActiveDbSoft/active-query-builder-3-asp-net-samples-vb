Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class VirtualObjectsAndFields
		Inherits BasePage
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("VirtualObjectsAndFields")

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
			Dim queryBuilder = QueryBuilderStore.Factory.MsSql("VirtualObjectsAndFields")
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = True
            
			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			Dim o As MetadataObject
			Dim f As MetadataField

			' Virtual fields for real object
			' ===========================================================================
			o = queryBuilder.MetadataContainer.FindItem(Of MetadataObject)("Orders")

			' first test field - simple expression
			f = o.AddField("OrderId_plus_1")
			f.Expression = "orders.OrderId + 1"

			' second test field - correlated sub-query
			f = o.AddField("CustomerCompanyName")
			f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = orders.CustomerId)"

			' Virtual object (table) with virtual fields
			' ===========================================================================

			o = queryBuilder.MetadataContainer.AddTable("MyOrders")
			o.Expression = "Orders"

			' first test field - simple expression
			f = o.AddField("OrderId_plus_1")
			f.Expression = "MyOrders.OrderId + 1"

			' second test field - correlated sub-query
			f = o.AddField("CustomerCompanyName")
			f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = MyOrders.CustomerId)"

			' Virtual object (sub-query) with virtual fields
			' ===========================================================================

			o = queryBuilder.MetadataContainer.AddTable("MyBetterOrders")
			o.Expression = "(select OrderId, CustomerId, OrderDate from Orders)"

			' first test field - simple expression
			f = o.AddField("OrderId_plus_1")
			f.Expression = "MyBetterOrders.OrderId + 1"

			' second test field - correlated sub-query
			f = o.AddField("CustomerCompanyName")
			f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = MyBetterOrders.CustomerId)"

			Dim sq As SQLQuery = queryBuilder.SQLQuery
			AddHandler sq.SQLUpdated, AddressOf OnSQLUpdated

			queryBuilder.SQL = "SELECT mbo.OrderId_plus_1, mbo.CustomerCompanyName FROM MyBetterOrders mbo"

			Return queryBuilder
		End Function

		Public Sub OnSQLUpdated(sender As Object, e As EventArgs)
			Dim qb = QueryBuilderStore.[Get]("VirtualObjectsAndFields")

			Dim opts = New SQLFormattingOptions()

			opts.Assign(qb.SQLFormattingOptions)
			opts.KeywordFormat = KeywordFormat.UpperCase

			' get query with virtual objects and fields
			opts.ExpandVirtualObjects = False
			Dim sqlWithVirtObjects = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts)

			' get SQL query with real object names
			opts.ExpandVirtualObjects = True
			Dim plainSql = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts)

			' prepare additional data to be sent to the client
			qb.ExchangeData = New With { _
				Key .SQL = plainSql, _
				Key .VirtualObjectsSQL = sqlWithVirtObjects _
			}
		End Sub
	End Class
End Namespace
