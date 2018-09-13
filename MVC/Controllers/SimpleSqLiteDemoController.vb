Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server
Imports MVC_Samples.Helpers

Namespace Controllers
	Public Class SimpleSqLiteDemoController
		Inherits Controller
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("SqLite")

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		''' <summary>
		''' Creates and initializes a new instance of the QueryBuilder object.
		''' </summary>
		''' <returns>Returns instance of the QueryBuilder object.</returns>
		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.SqLite("SqLite")

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = False
            
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
