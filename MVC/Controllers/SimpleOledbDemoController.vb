Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server
Imports MVC_Samples.Helpers

Namespace Controllers
	Public Class SimpleOledbDemoController
		Inherits Controller
		Const qbId As String = "Oledb"
		' identifies instance of the QueryBuilder object within a session
		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get](qbId)

			If qb Is Nothing Then
				qb = CreateQueryBuilder(qbId)
			End If

			Return View(qb)
		End Function

		''' <summary>
		''' Creates and initializes a new instance of the QueryBuilder object.
		''' </summary>
		''' <param name="AInstanceId">String which uniquely identifies an instance of Active Query Builder in the session.</param>
		''' <returns>Returns instance of the QueryBuilder object.</returns>
		Private Function CreateQueryBuilder(AInstanceId As String) As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Create(AInstanceId)

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = False

			' Assign an instance of the syntax provider which defines SQL syntax and metadata retrieval rules.
			queryBuilder.SyntaxProvider = New MSAccessSyntaxProvider()

			' Bind Active Query Builder to a live database connection.
				' Assign an instance of DBConnection object to the Connection property.
			queryBuilder.MetadataProvider = New OLEDBMetadataProvider() With { _
				.Connection = DataBaseHelper.CreateMSAccessConnection("NorthwindDataBase") _
			}

			' Assign the initial SQL query text the user sees on the _first_ page load
			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                      c.CustomerID As a1," & vbCr & vbLf & "                      c.CompanyName," & vbCr & vbLf & "                      s.ShipperID" & vbCr & vbLf & "                    From (Orders o" & vbCr & vbLf & "                      Inner Join Customers c On o.CustomerID = c.CustomerID)" & vbCr & vbLf & "                      Inner Join Shippers s On s.ShipperID = o.ShipperID" & vbCr & vbLf & "                    Where o.Ship_City = 'A'"
		End Function
	End Class
End Namespace
