Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class MetadataStructureController
		Inherits Controller
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("MetadataStructure")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Private Function CreateQueryBuilder() As QueryBuilder
			' Get instance of QueryBuilder
			Dim queryBuilder = QueryBuilderStore.Create("MetadataStructure")

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = True

			' Assign an instance of the syntax provider which defines SQL syntax and metadata retrieval rules.
			queryBuilder.SyntaxProvider = New MSSQLSyntaxProvider()

			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			' Initialization of the Metadata Structure object that's
			' responsible for representation of metadata in a tree-like form
			' Disable the automatic metadata structure creation
			queryBuilder.MetadataStructure.AllowChildAutoItems = False

			' queryBuilder.DatabaseSchemaTreeOptions.DefaultExpandLevel = 0;

			Dim filter As MetadataFilterItem

			' Create a top-level folder containing all objects
			Dim allObjects As New MetadataStructureItem()
			allObjects.Caption = "All objects"
			filter = allObjects.MetadataFilter.Add()
			filter.ObjectTypes = MetadataType.All
			queryBuilder.MetadataStructure.Items.Add(allObjects)

			' Create "Favorites" folder
			Dim favorites As New MetadataStructureItem()
			favorites.Caption = "Favorites"
			queryBuilder.MetadataStructure.Items.Add(favorites)

			Dim metadataItem As MetadataItem
			Dim item As MetadataStructureItem

			' Add some metadata objects to "Favorites" folder
			metadataItem = queryBuilder.MetadataContainer.FindItem(Of MetadataItem)("Orders")
			item = New MetadataStructureItem()
			item.MetadataItem = metadataItem
			favorites.Items.Add(item)

			metadataItem = queryBuilder.MetadataContainer.FindItem(Of MetadataItem)("Order Details")
			item = New MetadataStructureItem()
			item.MetadataItem = metadataItem
			favorites.Items.Add(item)

			' Create folder with filter
			Dim filteredFolder As New MetadataStructureItem()
			' creates dynamic node
			filteredFolder.Caption = "Filtered by 'Prod%'"
			filter = filteredFolder.MetadataFilter.Add()
			filter.ObjectTypes = MetadataType.Table Or MetadataType.View
			filter.[Object] = "Prod%"
			queryBuilder.MetadataStructure.Items.Add(filteredFolder)

			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "SELECT Orders.OrderID, Orders.CustomerID, Orders.OrderDate, [Order Details].ProductID," & vbCr & vbLf & vbTab & vbTab & vbTab & vbTab & vbTab & "[Order Details].UnitPrice, [Order Details].Quantity, [Order Details].Discount" & vbCr & vbLf & vbTab & vbTab & vbTab & vbTab & vbTab & "FROM Orders INNER JOIN [Order Details] ON Orders.OrderID = [Order Details].OrderID" & vbCr & vbLf & vbTab & vbTab & vbTab & vbTab & vbTab & "WHERE Orders.OrderID > 0 AND [Order Details].Discount > 0"
		End Function
	End Class
End Namespace
