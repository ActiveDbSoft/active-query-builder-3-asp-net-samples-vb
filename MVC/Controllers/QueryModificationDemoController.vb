Imports System.Collections.Generic
Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class QueryModificationDemoController
		Inherits Controller
		Private Const CustomersName As String = "Northwind.dbo.Customers"
		Private Const OrdersName As String = "Northwind.dbo.Orders"
		Private Const CustomersAlias As String = "c"
		Private Const OrdersAlias As String = "o"
		Private Const CustomersCompanyName As String = "CompanyName"
		Private Const CusomerId As String = "CustomerId"
		Private Const OrderDate As String = "OrderDate"

		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("QueryModification")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.MsSql("QueryModification")
            
			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			Return queryBuilder
		End Function

		Private Function IsTablePresentInQuery(unionSubQuery As UnionSubQuery, table As DataSource) As Boolean
			' collect the list of datasources used in FROM
			Dim dataSources = unionSubQuery.GetChildrenRecursive(Of DataSource)(False)

			' check given table in list of all datasources
			Return dataSources.IndexOf(table) <> -1
		End Function

		Private Function IsQueryColumnListItemPresentInQuery(unionSubQuery As UnionSubQuery, item As QueryColumnListItem) As Boolean
			Return unionSubQuery.QueryColumnList.IndexOf(item) <> -1 AndAlso Not [String].IsNullOrEmpty(item.ExpressionString)
		End Function

		Private Sub ClearConditionCells(unionSubQuery As UnionSubQuery, item As QueryColumnListItem)
			For i As Integer = 0 To unionSubQuery.QueryColumnList.GetMaxConditionCount() - 1
				item.ConditionStrings(i) = ""
			Next
		End Sub

		Private Function AddTable(unionSubQuery As UnionSubQuery, name As String, [alias] As String) As DataSource
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")

			Using parsedName = queryBuilder1.SQLContext.ParseQualifiedName(name)
				Using parsedAlias = queryBuilder1.SQLContext.ParseIdentifier([alias])
					Return queryBuilder1.SQLQuery.AddObject(unionSubQuery, parsedName, parsedAlias)
				End Using
			End Using
		End Function

		Private Function FindTableInQueryByName(unionSubQuery As UnionSubQuery, name As String) As DataSource
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")

			Dim foundDatasources As List(Of DataSourceObject)
			Using qualifiedName = queryBuilder1.SQLContext.ParseQualifiedName(name)
				foundDatasources = New List(Of DataSourceObject)()
				unionSubQuery.FromClause.FindTablesByDbName(qualifiedName, foundDatasources)
			End Using

			' if found more than one tables with given name in the query, use the first one
			Return If(foundDatasources.Count > 0, foundDatasources(0), Nothing)
		End Function

		Private Sub AddWhereCondition(columnList As QueryColumnList, whereListItem As QueryColumnListItem, condition As String)
			Dim whereFound As Boolean = False

			' GetMaxConditionCount: returns the number of non-empty criteria columns in the grid.
			For i As Integer = 0 To columnList.GetMaxConditionCount() - 1
				' CollectCriteriaItemsWithWhereCondition:
				' This function returns the list of conditions that were found in
				' the i-th criteria column, applied to specific clause (WHERE or HAVING).
				' Thus, this function collects all conditions joined with AND
				' within one OR group (one grid column).
				Dim foundColumnItems As New List(Of QueryColumnListItem)()
				CollectCriteriaItemsWithWhereCondition(columnList, i, foundColumnItems)

				' if found some conditions in i-th column, append condition to i-th column
				If foundColumnItems.Count > 0 Then
					whereListItem.ConditionStrings(i) = condition
					whereFound = True
				End If
			Next

			' if there are no cells with "where" conditions, add condition to new column
			If Not whereFound Then
				whereListItem.ConditionStrings(columnList.GetMaxConditionCount()) = condition
			End If
		End Sub

		Public Sub ApplyChanges(m As Model)
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")

			Dim customers As DataSource = Nothing
			Dim orders As DataSource = Nothing
			Dim _companyName As QueryColumnListItem = Nothing
			Dim _orderDate As QueryColumnListItem = Nothing

			'prepare parsed names
			Dim joinFieldName As SQLQualifiedName = queryBuilder1.SQLContext.ParseQualifiedName(CusomerId)
			Dim companyNameFieldName As SQLQualifiedName = queryBuilder1.SQLContext.ParseQualifiedName(CustomersCompanyName)
			Dim orderDateFieldName As SQLQualifiedName = queryBuilder1.SQLContext.ParseQualifiedName(OrderDate)

			' get the active SELECT

			Dim usq = queryBuilder1.ActiveUnionSubQuery

			'#Region "actualize stored references (if query is modified in GUI)"
			'#Region "actualize datasource references"
			' if user removed previously added datasources then clear their references
			If customers IsNot Nothing AndAlso Not IsTablePresentInQuery(usq, customers) Then
				' user removed this table in GUI
				customers = Nothing
			End If

			If orders IsNot Nothing AndAlso Not IsTablePresentInQuery(usq, orders) Then
				' user removed this table in GUI
				orders = Nothing
			End If
			'#End Region

			' clear CompanyName conditions
			If _companyName IsNot Nothing Then
				' if user removed entire row OR cleared expression cell in GUI, clear the stored reference
				If Not IsQueryColumnListItemPresentInQuery(usq, _companyName) Then
					_companyName = Nothing
				End If
			End If

			' clear all condition cells for CompanyName row
			If _companyName IsNot Nothing Then
				ClearConditionCells(usq, _companyName)
			End If

			' clear OrderDate conditions
			If _orderDate IsNot Nothing Then
				' if user removed entire row OR cleared expression cell in GUI, clear the stored reference
				If Not IsQueryColumnListItemPresentInQuery(usq, _orderDate) Then
					_orderDate = Nothing
				End If
			End If

			' clear all condition cells for OrderDate row
			If _orderDate IsNot Nothing Then
				ClearConditionCells(usq, _orderDate)
			End If
			'#End Region

			'#Region "process Customers table"
			If m.Customers Then
				' if we have no previously added Customers table, try to find one already added by the user
				If customers Is Nothing Then
					customers = FindTableInQueryByName(usq, CustomersName)
				End If

				' there is no Customers table in query, add it
				If customers Is Nothing Then
					customers = AddTable(usq, CustomersName, CustomersAlias)
				End If

				'#Region "process CompanyName condition"
				If m.CompanyName AndAlso Not [String].IsNullOrEmpty(m.CompanyNameText) Then
					' if we have no previously added grid row for this condition, add it
					If _companyName Is Nothing OrElse _companyName.IsDisposing Then
						_companyName = usq.QueryColumnList.AddField(customers, companyNameFieldName.QualifiedName)
						' do not append it to the select list, use this row for conditions only
						_companyName.Selected = False
					End If

					' write condition from edit box to all needed grid cells
					AddWhereCondition(usq.QueryColumnList, _companyName, m.CompanyNameText)
				Else
					' remove previously added grid row
					If _companyName IsNot Nothing Then
						_companyName.Dispose()
					End If

					_companyName = Nothing
					'#End Region
				End If
			Else
				' remove previously added datasource
				If customers IsNot Nothing Then
					customers.Dispose()
				End If

				customers = Nothing
			End If
			'#End Region

			'#Region "process Orders table"
			If m.Orders Then
				' if we have no previosly added Orders table, try to find one already added by the user
				If orders Is Nothing Then
					orders = FindTableInQueryByName(usq, OrdersName)
				End If

				' there are no Orders table in query, add one
				If orders Is Nothing Then
					orders = AddTable(usq, OrdersName, OrdersAlias)
				End If

				'#Region "link between Orders and Customers"
				' we added Orders table,
				' check if we have Customers table too,
				' and if there are no joins between them, create such join
				Dim joinFieldNameStr As String = joinFieldName.QualifiedName
				If customers IsNot Nothing AndAlso usq.FromClause.FindLink(orders, joinFieldNameStr, customers, joinFieldNameStr) Is Nothing AndAlso usq.FromClause.FindLink(customers, joinFieldNameStr, orders, joinFieldNameStr) Is Nothing Then
					queryBuilder1.SQLQuery.AddLink(customers, joinFieldName, orders, joinFieldName)
				End If
				'#End Region

				'#Region "process OrderDate condition"
				If m.OrderDate AndAlso Not [String].IsNullOrEmpty(m.OrderDateText) Then
					' if we have no previously added grid row for this condition, add it
					If _orderDate Is Nothing Then
						_orderDate = usq.QueryColumnList.AddField(orders, orderDateFieldName.QualifiedName)
						' do not append it to the select list, use this row for conditions only
						_orderDate.Selected = False
					End If

					' write condition from edit box to all needed grid cells
					AddWhereCondition(usq.QueryColumnList, _orderDate, m.OrderDateText)
				Else
					' remove prviously added grid row
					If _orderDate IsNot Nothing Then
						_orderDate.Dispose()
					End If

					_orderDate = Nothing
					'#End Region
				End If
			Else
				If orders IsNot Nothing Then
					orders.Dispose()
					orders = Nothing
				End If
			End If
			'#End Region
		End Sub

		Private Sub CollectCriteriaItemsWithWhereCondition(criteriaList As QueryColumnList, columnIndex As Integer, result As IList(Of QueryColumnListItem))
			result.Clear()

			For Each item As QueryColumnListItem In criteriaList
				If item.ConditionType = ConditionType.Where AndAlso item.ConditionCount > columnIndex AndAlso item.GetASTCondition(columnIndex) IsNot Nothing Then
					result.Add(item)
				End If
			Next
		End Sub
	End Class

	Public Class Model
		Public Property Customers() As Boolean
			Get
				Return m_Customers
			End Get
			Set
				m_Customers = Value
			End Set
		End Property
		Private m_Customers As Boolean
		Public Property CompanyName() As Boolean
			Get
				Return m_CompanyName
			End Get
			Set
				m_CompanyName = Value
			End Set
		End Property
		Private m_CompanyName As Boolean
		Public Property CompanyNameText() As String
			Get
				Return m_CompanyNameText
			End Get
			Set
				m_CompanyNameText = Value
			End Set
		End Property
		Private m_CompanyNameText As String
		Public Property Orders() As Boolean
			Get
				Return m_Orders
			End Get
			Set
				m_Orders = Value
			End Set
		End Property
		Private m_Orders As Boolean
		Public Property OrderDate() As Boolean
			Get
				Return m_OrderDate
			End Get
			Set
				m_OrderDate = Value
			End Set
		End Property
		Private m_OrderDate As Boolean
		Public Property OrderDateText() As String
			Get
				Return m_OrderDateText
			End Get
			Set
				m_OrderDateText = Value
			End Set
		End Property
		Private m_OrderDateText As String
	End Class
End Namespace
