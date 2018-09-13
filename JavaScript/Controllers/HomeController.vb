Imports System.Web.Mvc
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class HomeController
		Inherits Controller
		Public Function Index() As ActionResult
			Return View()
		End Function

		' //CUT:PRO{{
		Public Sub Dispose()
		    QueryBuilderStore.Remove()
		    QueryTransformerStore.Remove()
		End Sub
		' //}}CUT:PRO
	End Class
End Namespace
