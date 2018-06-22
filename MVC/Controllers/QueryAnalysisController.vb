Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Configuration
Imports System.Data
Imports System.IO
Imports System.Text
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class QueryAnalysisController
		Inherits Controller
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("QueryAnalysis")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.MsSql("QueryAnalysis")
            
			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)

			Dim sq As SQLQuery = queryBuilder.SQLQuery
			AddHandler sq.SQLUpdated, AddressOf OnSQLUpdated

			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity" & vbCr & vbLf & "                From Orders o Inner Join" & vbCr & vbLf & "                  Customers c On o.Customer_ID = c.ID Inner Join" & vbCr & vbLf & "                  Shippers s On s.ID = o.Shipper_ID" & vbCr & vbLf & "                Where o.ShipCity = 'A'"
		End Function

		Public Sub OnSQLUpdated(sender As Object, e As EventArgs)
			Dim queryBuilder = QueryBuilderStore.[Get]("QueryAnalysis")

			Dim data = New ExchangeClass()

			data.Statistics = GetQueryStatistic(queryBuilder.QueryStatistics)
			data.SubQueries = DumpSubQueries(queryBuilder)
			data.QueryStructure = DumpQueryStructureInfo(queryBuilder.ActiveSubQuery)
			data.UnionSubQuery = New UnionSubQueryExchangeClass()

			data.UnionSubQuery.SelectedExpressions = DumpSelectedExpressionsInfoFromUnionSubQuery(queryBuilder.ActiveUnionSubQuery)
			data.UnionSubQuery.DataSources = DumpDataSourcesInfoFromUnionSubQuery(queryBuilder.ActiveUnionSubQuery)
			

			data.UnionSubQuery.Links = DumpLinksInfoFromUnionSubQuery(queryBuilder.ActiveUnionSubQuery)
			data.UnionSubQuery.Where = GetWhereInfo(queryBuilder.ActiveUnionSubQuery)

			queryBuilder.ExchangeData = data
		End Sub

		Friend Class UnionSubQueryExchangeClass
			Public SelectedExpressions As String
			Public DataSources As String
			Public Links As String
			Public Where As String
		End Class

		Private Class ExchangeClass
			Public Statistics As String
			Public SubQueries As String
			Public QueryStructure As String
			Public UnionSubQuery As UnionSubQueryExchangeClass
		End Class


		Private Function GetQueryStatistic(qs As QueryStatistics) As String
			Dim stats As String = ""

			stats = "<b>Used Objects (" & Convert.ToString(qs.UsedDatabaseObjects.Count) & "):</b><br/>"
			For i As Integer = 0 To qs.UsedDatabaseObjects.Count - 1
				stats += "<br />" + qs.UsedDatabaseObjects(i).ObjectName.QualifiedName
			Next

			stats += "<br /><br />" & "<b>Used Columns (" & Convert.ToString(qs.UsedDatabaseObjectFields.Count) & "):</b><br />"
			For i As Integer = 0 To qs.UsedDatabaseObjectFields.Count - 1
				stats += "<br />" + qs.UsedDatabaseObjectFields(i).FullName.QualifiedName
			Next

			stats += "<br /><br />" & "<b>Output Expressions (" & Convert.ToString(qs.OutputColumns.Count) & "):</b><br />"
			For i As Integer = 0 To qs.OutputColumns.Count - 1
				stats += "<br />" + qs.OutputColumns(i).Expression
			Next
			Return stats
		End Function

		Private Function DumpQueryStructureInfo(subQuery As SubQuery) As String
			Dim stringBuilder = New StringBuilder()
			DumpUnionGroupInfo(stringBuilder, "", subQuery)
			Return stringBuilder.ToString()
		End Function

		Private Sub DumpUnionGroupInfo(stringBuilder As StringBuilder, indent As String, unionGroup As UnionGroup)
			Dim children As QueryBase() = GetUnionChildren(unionGroup)

			For Each child As QueryBase In children
				If stringBuilder.Length > 0 Then
					stringBuilder.AppendLine("<br />")
				End If

				If TypeOf child Is UnionSubQuery Then
					' UnionSubQuery is a leaf node of query structure.
					' It represent a single SELECT statement in the tree of unions
					DumpUnionSubQueryInfo(stringBuilder, indent, DirectCast(child, UnionSubQuery))
				Else
					' UnionGroup is a tree node.
					' It contains one or more leafs of other tree nodes.
					' It represent a root of the subquery of the union tree or a
					' parentheses in the union tree.
					unionGroup = DirectCast(child, UnionGroup)

					stringBuilder.AppendLine(indent & Convert.ToString(unionGroup.UnionOperatorFull) & "group: [")
					DumpUnionGroupInfo(stringBuilder, indent & "&nbsp;&nbsp;&nbsp;&nbsp;", unionGroup)
					stringBuilder.AppendLine(indent & "]<br />")
				End If
			Next
		End Sub

		Private Sub DumpUnionSubQueryInfo(stringBuilder As StringBuilder, indent As String, unionSubQuery As UnionSubQuery)
			Dim sql As String = unionSubQuery.GetResultSQL()

			stringBuilder.AppendLine(indent & Convert.ToString(unionSubQuery.UnionOperatorFull) & ": " & sql & "<br />")
		End Sub

		Private Function GetUnionChildren(unionGroup As UnionGroup) As QueryBase()
			Dim result As New ArrayList()

			For i As Integer = 0 To unionGroup.Count - 1
				result.Add(unionGroup(i))
			Next

			Return DirectCast(result.ToArray(GetType(QueryBase)), QueryBase())
		End Function

		Public Function DumpSelectedExpressionsInfoFromUnionSubQuery(unionSubQuery As UnionSubQuery) As String
			Dim stringBuilder = New StringBuilder()
			' get list of CriteriaItems
			Dim criteriaList As QueryColumnList = unionSubQuery.QueryColumnList

			' dump all items
			For i As Integer = 0 To criteriaList.Count - 1
				Dim criteriaItem As QueryColumnListItem = criteriaList(i)

				' only items have .Select property set to True goes to SELECT list
				If Not criteriaItem.Selected Then
					Continue For
				End If

				' separator
				If stringBuilder.Length > 0 Then
					stringBuilder.AppendLine("<br />")
				End If

				DumpSelectedExpressionInfo(stringBuilder, criteriaItem)
			Next

			Return stringBuilder.ToString()
		End Function

		Private Sub DumpSelectedExpressionInfo(stringBuilder As StringBuilder, selectedExpression As QueryColumnListItem)
			' write full sql fragment of selected expression
			stringBuilder.AppendLine(Convert.ToString(selectedExpression.ExpressionString) & "<br />")

			' write alias
			If Not [String].IsNullOrEmpty(selectedExpression.AliasString) Then
				stringBuilder.AppendLine("&nbsp;&nbsp;alias: " & Convert.ToString(selectedExpression.AliasString) & "<br />")
			End If

			' write datasource reference (if any)
			If selectedExpression.ExpressionDatasource IsNot Nothing Then
				stringBuilder.AppendLine("&nbsp;&nbsp;datasource: " & selectedExpression.ExpressionDatasource.GetResultSQL() & "<br />")
			End If

			' write metadata information (if any)
			If selectedExpression.ExpressionField IsNot Nothing Then
				Dim field As MetadataField = selectedExpression.ExpressionField
				stringBuilder.AppendLine("&nbsp;&nbsp;field name: " + field.Name & "<br />")

				Dim s As String = [Enum].GetName(GetType(DbType), field.FieldType)
				stringBuilder.AppendLine("&nbsp;&nbsp;field type: " & s & "<br />")
			End If
		End Sub

		Private Sub DumpDataSourcesInfo(stringBuilder As StringBuilder, dataSources As IList(Of DataSource))
			For i As Integer = 0 To dataSources.Count - 1
				If stringBuilder.Length > 0 Then
					stringBuilder.AppendLine("<br />")
				End If

				DumpDataSourceInfo(stringBuilder, dataSources(i))
			Next
		End Sub

		Public Function DumpDataSourcesInfoFromUnionSubQuery(unionSubQuery As UnionSubQuery) As String
			Dim stringBuilder As New StringBuilder()
			DumpDataSourcesInfo(stringBuilder, GetDataSourceList(unionSubQuery))
			Return stringBuilder.ToString()
		End Function

		Private Function GetDataSourceList(unionSubQuery As UnionSubQuery) As List(Of DataSource)
			Dim list As New List(Of DataSource)()

			unionSubQuery.FromClause.GetDatasourceByClass(Of DataSource)(list)

			Return list
		End Function

		Private Sub DumpDataSourceInfo(stringBuilder As StringBuilder, dataSource As DataSource)
			' write full sql fragment
			stringBuilder.AppendLine("<b>" & dataSource.GetResultSQL() & "</b><br />")

			' write alias
			stringBuilder.AppendLine("&nbsp;&nbsp;alias: " & Convert.ToString(dataSource.[Alias]) & "<br />")

			' write referenced MetadataObject (if any)
			If dataSource.MetadataObject IsNot Nothing Then
				stringBuilder.AppendLine("&nbsp;&nbsp;ref: " & Convert.ToString(dataSource.MetadataObject.Name) & "<br />")
			End If

			' write subquery (if datasource is actually a derived table)
			If TypeOf dataSource Is DataSourceQuery Then
				stringBuilder.AppendLine("&nbsp;&nbsp;subquery sql: " & DirectCast(dataSource, DataSourceQuery).SubQuery.GetResultSQL() & "<br />")
			End If

			' write fields
			Dim fields As String = [String].Empty

			For i As Integer = 0 To dataSource.Metadata.Count - 1
				If fields.Length > 0 Then
					fields += ", "
				End If

				fields += dataSource.Metadata(i).Name
			Next

			stringBuilder.AppendLine("&nbsp;&nbsp;fields (" & dataSource.Metadata.Count.ToString() & "): " & fields & "<br />")
		End Sub

		Private Sub DumpLinkInfo(stringBuilder As StringBuilder, link As Link)
			' write full sql fragment of link expression
			stringBuilder.AppendLine(link.LinkExpression.GetSQL(link.SQLContext.SQLGenerationOptionsForServer) & "<br />")

			' write information about left side of link
			stringBuilder.AppendLine("&nbsp;&nbsp;left datasource: " & link.LeftDataSource.GetResultSQL() & "<br />")

			If link.LeftType = LinkSideType.Inner Then
				stringBuilder.AppendLine("&nbsp;&nbsp;left type: Inner" & "<br />")
			Else
				stringBuilder.AppendLine("&nbsp;&nbsp;left type: Outer" & "<br />")
			End If

			' write information about right side of link
			stringBuilder.AppendLine("&nbsp;&nbsp;right datasource: " & link.RightDataSource.GetResultSQL() & "<br />")

			If link.RightType = LinkSideType.Inner Then
				stringBuilder.AppendLine("&nbsp;&nbsp;lerightft type: Inner" & "<br />")
			Else
				stringBuilder.AppendLine("&nbsp;&nbsp;right type: Outer" & "<br />")
			End If
		End Sub

		Private Sub DumpLinksInfo(stringBuilder As StringBuilder, links As IList(Of Link))
			For i As Integer = 0 To links.Count - 1
				If stringBuilder.Length > 0 Then
					stringBuilder.AppendLine("<br />")
				End If

				DumpLinkInfo(stringBuilder, DirectCast(links(i), Link))
			Next
		End Sub

		Private Function GetLinkList(unionSubQuery As UnionSubQuery) As IList(Of Link)
			Dim links As New List(Of Link)()

			unionSubQuery.FromClause.GetLinksRecursive(links)

			Return links
		End Function

		Public Function DumpLinksInfoFromUnionSubQuery(unionSubQuery As UnionSubQuery) As String
			Dim stringBuilder = New StringBuilder()
			DumpLinksInfo(stringBuilder, GetLinkList(unionSubQuery))
			Return stringBuilder.ToString()
		End Function

		Public Sub DumpWhereInfo(stringBuilder As StringBuilder, where As SQLExpressionItem)
			DumpExpression(stringBuilder, "", where)
		End Sub

		Private Sub DumpExpression(stringBuilder As StringBuilder, indent As String, expression As SQLExpressionItem)
			Const  cIndentInc As String = "&nbsp;&nbsp;&nbsp;&nbsp;"

			Dim newIndent As String = indent & cIndentInc

			If expression Is Nothing Then
				' NULL reference protection
				stringBuilder.AppendLine(indent & "--nil--" & "<br />")
			ElseIf TypeOf expression Is SQLExpressionBrackets Then
				' Expression is actually the brackets query structure node.
				' Create the "brackets" tree node and load content of
				' the brackets as children of the node.
				stringBuilder.AppendLine(indent & "()" & "<br />")
				DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionBrackets).LExpression)
			ElseIf TypeOf expression Is SQLExpressionOr Then
				' Expression is actually the "OR" query structure node.
				' Create the "OR" tree node and load all items of
				' the "OR" collection as children of the tree node.
				stringBuilder.AppendLine(indent & "OR" & "<br />")

				For i As Integer = 0 To DirectCast(expression, SQLExpressionOr).Count - 1
					DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionOr)(i))
				Next
			ElseIf TypeOf expression Is SQLExpressionAnd Then
				' Expression is actually the "AND" query structure node.
				' Create the "AND" tree node and load all items of
				' the "AND" collection as children of the tree node.
				stringBuilder.AppendLine(indent & "AND" & "<br />")

				For i As Integer = 0 To DirectCast(expression, SQLExpressionAnd).Count - 1
					DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionAnd)(i))
				Next
			ElseIf TypeOf expression Is SQLExpressionNot Then
				' Expression is actually the "NOT" query structure node.
				' Create the "NOT" tree node and load content of
				' the "NOT" operator as children of the tree node.
				stringBuilder.AppendLine(indent & "NOT" & "<br />")
				DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionNot).LExpression)
			ElseIf TypeOf expression Is SQLExpressionOperatorBinary Then
				' Expression is actually the "BINARY OPERATOR" query structure node.
				' Create a tree node containing the operator value and
				' two leaf nodes with the operator arguments.
				Dim s As String = DirectCast(expression, SQLExpressionOperatorBinary).OperatorObj.OperatorName
				stringBuilder.AppendLine(indent & s & "<br />")
				' left argument of the binary operator
				DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionOperatorBinary).LExpression)
				' right argument of the binary operator
				DumpExpression(stringBuilder, newIndent, DirectCast(expression, SQLExpressionOperatorBinary).RExpression)
			Else
				' other type of AST nodes - out as a text
				Dim s As String = expression.GetSQL(expression.SQLContext.SQLGenerationOptionsForServer)
				stringBuilder.AppendLine(indent & s & "<br />")
			End If
		End Sub

		Private Function GetWhereInfo(unionSubQuery As UnionSubQuery) As String
			Dim stringBuilder As New StringBuilder()

			Dim unionSubQueryAst As SQLSubQuerySelectExpression = unionSubQuery.ResultQueryAST

			Try
				If unionSubQueryAst.Where IsNot Nothing Then
					DumpWhereInfo(stringBuilder, unionSubQueryAst.Where)
				End If
			Finally
				unionSubQueryAst.Dispose()
			End Try

			Return stringBuilder.ToString()
		End Function

		Public Function DumpSubQueries(queryBuilder As QueryBuilder) As String
			Dim stringBuilder As New StringBuilder()
			DumpSubQueriesInfo(stringBuilder, queryBuilder)
			Return stringBuilder.ToString()
		End Function

		Private Sub DumpSubQueryInfo(stringBuilder As StringBuilder, index As Integer, subQuery As SubQuery)
			Dim sql As String = subQuery.GetResultSQL()

			stringBuilder.AppendLine(index & ": " & sql & "<br />")
		End Sub

		Public Sub DumpSubQueriesInfo(stringBuilder As StringBuilder, queryBuilder As QueryBuilder)
			For i As Integer = 0 To queryBuilder.SQLQuery.QueryRoot.SubQueries.Count - 1
				If stringBuilder.Length > 0 Then
					stringBuilder.AppendLine("<br />")
				End If

				DumpSubQueryInfo(stringBuilder, i, queryBuilder.SQLQuery.QueryRoot.SubQueries(i))
			Next
		End Sub
	End Class
End Namespace
