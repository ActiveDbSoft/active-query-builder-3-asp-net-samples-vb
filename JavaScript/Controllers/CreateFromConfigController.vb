Imports System.Web.Mvc
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class CreateFromConfigController
		Inherits Controller
		Public Function Index() As ActionResult
			Return View()
		End Function

		''' <summary>
		''' Creates and initializes new instance of the QueryBuilder object for the given identifier if it doesn't exist. 
		''' </summary>
		''' <param name="name">Instance identifier of object in the current session.</param>
		''' <returns></returns>
		Public Function Create(name As String) As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get](name)

			If qb IsNot Nothing Then
				Return New EmptyResult()
			End If

			' Create an instance of the QueryBuilder object
			qb = QueryBuilderStore.Create(name)

			' The necessary initialization procedures to setup SQL syntax and the source of metadata will be performed automatically 
			' according to directives in the special configuration section of 'Web.config' file.

			' This behavior is enabled by the SessionStore.WebConfig() method call in the Application_Start method in Global.asax.cs file.

			' Set default query
			qb.SQL = GetDefaultSql()

			Return New EmptyResult()
		End Function
		Private Function GetDefaultSql() As String
			Return "Select o.OrderID," & vbCr & vbLf & "                        c.CustomerID," & vbCr & vbLf & "                        s.ShipperID," & vbCr & vbLf & "                        o.ShipCity" & vbCr & vbLf & "                    From Orders o" & vbCr & vbLf & "                        Inner Join Customers c On o.CustomerID = c.CustomerID" & vbCr & vbLf & "                        Inner Join Shippers s On s.ShipperID = o.OrderID" & vbCr & vbLf & "                    Where o.ShipCity = 'A'"
		End Function
	End Class
End Namespace
