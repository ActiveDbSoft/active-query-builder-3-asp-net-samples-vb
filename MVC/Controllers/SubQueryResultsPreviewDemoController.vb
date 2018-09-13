Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Core.QueryTransformer
Imports ActiveQueryBuilder.Web.Server
Imports MVC_Samples.Helpers

Namespace Controllers
	Public Class SubQueryResultsPreviewDemoController
		Inherits Controller
		Private Shared Name As String = "SubQueryResultsPreview"

		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get](Name)

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			Return View(qb)
		End Function

		Public Function GetData() As ActionResult
			Dim qb As QueryBuilder = QueryBuilderStore.[Get](Name)
			Dim conn = qb.MetadataProvider.Connection

			Dim sqlQuery = New SQLQuery(qb.SQLContext) With { _
				.SQL = qb.ActiveUnionSubQuery.SQL _
			}

			Dim qt As New QueryTransformer() With { _
				.QueryProvider = sqlQuery _
			}

			qt.Take("7")

			Dim columns = qt.Columns.[Select](Function(c) c.ResultName).ToList()

			Try
				Dim data = DataBaseHelper.GetDataList(conn, qt.SQL)
				Dim result = New With { _
					columns, _
					data _
				}
				Return Json(result, JsonRequestBehavior.AllowGet)
			Catch e As Exception
				Dim result = New With { _
					columns, _
					Key .data = New List(Of List(Of Object))() From { _
						New List(Of Object)() From { _
							e.Message _
						} _
					} _
				}
				Return Json(result, JsonRequestBehavior.AllowGet)
			End Try
		End Function

		''' <summary>
		''' Creates and initializes a new instance of the QueryBuilder object.
		''' </summary>
		''' <returns>Returns instance of the QueryBuilder object.</returns>
		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.SqLite(Name)

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
			Return "Select Count(Query1.EmployeeId) As Count_L," & vbCr & vbLf & "                      Count(Query2.EmployeeId) As Count_C" & vbCr & vbLf & "                    From (Select employees.EmployeeId," & vbCr & vbLf & "                            employees.LastName," & vbCr & vbLf & "                            employees.FirstName," & vbCr & vbLf & "                            employees.City" & vbCr & vbLf & "                          From employees" & vbCr & vbLf & "                          Where employees.City = 'Lethbridge') Query1," & vbCr & vbLf & "                      (Select employees.EmployeeId," & vbCr & vbLf & "                            employees.LastName," & vbCr & vbLf & "                            employees.FirstName," & vbCr & vbLf & "                            employees.City" & vbCr & vbLf & "                          From employees" & vbCr & vbLf & "                          Where employees.City = 'Calgary') Query2"
		End Function
	End Class
End Namespace
