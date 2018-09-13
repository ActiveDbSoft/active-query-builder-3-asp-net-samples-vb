Imports System.Web.Mvc
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		' GET
		Public Function Index() As ActionResult
			' We've redefined the QueryBuilderStore.Provider object to be of QueryBuilderSqliteStoreProvider class in the Global.asax.cs file.
			' The implementation of Get method in this provider gets _OR_creates_new_ QueryBuilder object.
			' The initialization of the QueryBuilder object is also internally made by the QueryBuilderSqliteStoreProvider.
			Dim qb = QueryBuilderStore.[Get]()
			Return View(qb)
		End Function
	End Class
End Namespace
