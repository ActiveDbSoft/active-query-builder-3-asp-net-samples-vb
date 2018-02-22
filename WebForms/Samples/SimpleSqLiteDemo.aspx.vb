Imports System.Web.UI
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server
Imports WebForms_Samples.Helpers

Namespace Samples
	Public Partial Class SimpleSqLiteDemo
		Inherits Page
		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("SqLite")

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

		''' <summary>
		''' Creates and initializes a new instance of the QueryBuilder object.
		''' </summary>
		''' <returns>Returns instance of the QueryBuilder object.</returns>
		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Create("SqLite")

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = False

			' Assign an instance of the syntax provider which defines SQL syntax and metadata retrieval rules.
			queryBuilder.SyntaxProvider = New SQLiteSyntaxProvider()

			' Bind Active Query Builder to a live database connection.
				' Assign an instance of DBConnection object to the Connection property.
			queryBuilder.MetadataProvider = New SQLiteMetadataProvider() With { _
				.Connection = DataBaseHelper.CreateSqLiteConnection("SqLiteDataBase") _
			}

			' Assign the initial SQL query text the user sees on the _first_ page load
			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "Select customers.CustomerId," & vbCr & vbLf & "                      customers.LastName," & vbCr & vbLf & "                      customers.FirstName" & vbCr & vbLf & "                    From customers"
		End Function
	End Class
End Namespace
