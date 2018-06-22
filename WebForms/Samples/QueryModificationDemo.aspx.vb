Imports System.Collections.Generic
Imports System.Configuration
Imports System.IO
Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Samples
	Public Partial Class QueryModificationDemo
		Inherits BasePage
		Private _joinFieldName As SQLQualifiedName
		Private _companyNameFieldName As SQLQualifiedName
		Private _orderDateFieldName As SQLQualifiedName

		Private _customers As DataSource
		Private _orders As DataSource
		Private _companyName As QueryColumnListItem
		Private _orderDate As QueryColumnListItem

		Private Const CustomersName As String = "Northwind.dbo.Customers"
		Private Const OrdersName As String = "Northwind.dbo.Orders"
		Private Const CustomersAlias As String = "c"
		Private Const OrdersAlias As String = "o"
		Private Const CustomersCompanyName As String = "CompanyName"
		Private Const CusomerId As String = "CustomerId"
		Private Const OrderDate As String = "OrderDate"

		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("QueryModification")

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

			cbCompanyName.Enabled = cbCustomers.Checked
			tbCompanyName.Enabled = cbCompanyName.Checked

			cbOrderDate.Enabled = cbOrders.Checked
			tbOrderDate.Enabled = cbOrderDate.Checked

			'prepare parsed names
			_joinFieldName = qb.SQLContext.ParseQualifiedName(CusomerId)
			_companyNameFieldName = qb.SQLContext.ParseQualifiedName(CustomersCompanyName)
			_orderDateFieldName = qb.SQLContext.ParseQualifiedName(OrderDate)
		End Sub

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

		Protected Sub btnApply_Click(sender As Object, e As EventArgs)
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")

			' get the active SELECT

			Dim usq = queryBuilder1.ActiveUnionSubQuery

			'#Region "actualize stored references (if query is modified in GUI)"
			'#Region "actualize datasource references"
			' if user removed previously added datasources then clear their references
			If _customers IsNot Nothing AndAlso Not IsTablePresentInQuery(usq, _customers) Then
				' user removed this table in GUI
				_customers = Nothing
			End If

			If _orders IsNot Nothing AndAlso Not IsTablePresentInQuery(usq, _orders) Then
				' user removed this table in GUI
				_orders = Nothing
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
			If cbCustomers.Checked Then
				' if we have no previously added Customers table, try to find one already added by the user
				If _customers Is Nothing Then
					_customers = FindTableInQueryByName(usq, CustomersName)
				End If

				' there is no Customers table in query, add it
				If _customers Is Nothing Then
					_customers = AddTable(usq, CustomersName, CustomersAlias)
				End If

				'#Region "process CompanyName condition"
				If cbCompanyName.Enabled AndAlso cbCompanyName.Checked AndAlso Not [String].IsNullOrEmpty(tbCompanyName.Text) Then
					' if we have no previously added grid row for this condition, add it
					If _companyName Is Nothing OrElse _companyName.IsDisposing Then
						_companyName = usq.QueryColumnList.AddField(_customers, _companyNameFieldName.QualifiedName)
						' do not append it to the select list, use this row for conditions only
						_companyName.Selected = False
					End If

					' write condition from edit box to all needed grid cells
					AddWhereCondition(usq.QueryColumnList, _companyName, tbCompanyName.Text)
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
				If _customers IsNot Nothing Then
					_customers.Dispose()
				End If

				_customers = Nothing
			End If
			'#End Region

			'#Region "process Orders table"
			If cbOrders.Checked Then
				' if we have no previosly added Orders table, try to find one already added by the user
				If _orders Is Nothing Then
					_orders = FindTableInQueryByName(usq, OrdersName)
				End If

				' there are no Orders table in query, add one
				If _orders Is Nothing Then
					_orders = AddTable(usq, OrdersName, OrdersAlias)
				End If

				'#Region "link between Orders and Customers"
				' we added Orders table,
				' check if we have Customers table too,
				' and if there are no joins between them, create such join
				Dim joinFieldNameStr As String = _joinFieldName.QualifiedName
				If _customers IsNot Nothing AndAlso usq.FromClause.FindLink(_orders, joinFieldNameStr, _customers, joinFieldNameStr) Is Nothing AndAlso usq.FromClause.FindLink(_customers, joinFieldNameStr, _orders, joinFieldNameStr) Is Nothing Then
					queryBuilder1.SQLQuery.AddLink(_customers, _joinFieldName, _orders, _joinFieldName)
				End If
				'#End Region

				'#Region "process OrderDate condition"
				If cbOrderDate.Enabled AndAlso cbOrderDate.Checked AndAlso Not [String].IsNullOrEmpty(tbOrderDate.Text) Then
					' if we have no previously added grid row for this condition, add it
					If _orderDate Is Nothing Then
						_orderDate = usq.QueryColumnList.AddField(_orders, _orderDateFieldName.QualifiedName)
						' do not append it to the select list, use this row for conditions only
						_orderDate.Selected = False
					End If

					' write condition from edit box to all needed grid cells
					AddWhereCondition(usq.QueryColumnList, _orderDate, tbOrderDate.Text)
				Else
					' remove prviously added grid row
					If _orderDate IsNot Nothing Then
						_orderDate.Dispose()
					End If

					_orderDate = Nothing
					'#End Region
				End If
			Else
				If _orders IsNot Nothing Then
					_orders.Dispose()
					_orders = Nothing
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

		Protected Sub btnQueryCustomers_Click(sender As Object, e As EventArgs)
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")
			queryBuilder1.SQL = "select * from Northwind.dbo.Customers c"
		End Sub

		Protected Sub btnQueryOrders_Click(sender As Object, e As EventArgs)
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")
			queryBuilder1.SQL = "select * from Northwind.dbo.Orders o"
		End Sub

		Protected Sub btnQueryCustomersOrders_Click(sender As Object, e As EventArgs)
			Dim queryBuilder1 = QueryBuilderStore.[Get]("QueryModification")
			queryBuilder1.SQL = "select * from Northwind.dbo.Customers c, Northwind.dbo.Orders o"
		End Sub
	End Class
End Namespace
