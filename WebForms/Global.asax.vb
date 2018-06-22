Imports System.Web
Imports ActiveQueryBuilder.Web.Server.Handlers
Imports log4net

Public Class [Global]
	Inherits HttpApplication
	Private Sub Application_Start(sender As Object, e As EventArgs)
		BaseHandler.Log = New Log()
	End Sub

	Private Sub Application_Error(sender As Object, e As EventArgs)
		Dim s = Server.GetLastError()
	End Sub
End Class

Public Class Log
	Implements ActiveQueryBuilder.Core.ILog
	Private Shared ReadOnly _Log As ILog = LogManager.GetLogger("Logger")

	Public Sub Trace(message As String) Implements ActiveQueryBuilder.Core.ILog.Trace
		_Log.Info(message)
	End Sub

	Public Sub Warning(message As String) Implements ActiveQueryBuilder.Core.ILog.Warning
		_Log.Warn(message)
	End Sub

	Public Sub [Error](message As String) Implements ActiveQueryBuilder.Core.ILog.Error
		_Log.[Error](message)
	End Sub

	Public Sub [Error](message As String, ex As Exception) Implements ActiveQueryBuilder.Core.ILog.Error
		_Log.[Error](message, ex)
	End Sub
End Class
