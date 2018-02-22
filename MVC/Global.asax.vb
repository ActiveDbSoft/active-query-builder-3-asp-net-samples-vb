Imports System.Web
Imports System.Web.Mvc
Imports System.Web.Routing
Imports ActiveQueryBuilder.Web.Server.Handlers
Imports log4net

Public Class MvcApplication
	Inherits HttpApplication
	Protected Sub Application_Start()
		AreaRegistration.RegisterAllAreas()
		FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
		RouteConfig.RegisterRoutes(RouteTable.Routes)

		BaseHandler.Log = New Logger()
	End Sub
End Class

Public Class Logger
	Implements ActiveQueryBuilder.Core.ILog
	Private Shared ReadOnly Log As ILog = LogManager.GetLogger("Logger")

	Public Sub Trace(message As String) Implements ActiveQueryBuilder.Core.ILog.Trace
		Log.Info(message)
	End Sub

	Public Sub Warning(message As String) Implements ActiveQueryBuilder.Core.ILog.Warning
		Log.Warn(message)
	End Sub

	Public Sub [Error](message As String) Implements ActiveQueryBuilder.Core.ILog.Error
		Log.[Error](message)
	End Sub

	Public Sub [Error](ex As Exception, message As String) Implements ActiveQueryBuilder.Core.ILog.Error
		Log.[Error](message, ex)
	End Sub
End Class
