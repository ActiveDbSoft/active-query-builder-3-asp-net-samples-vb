Imports System.Collections.Generic
Imports System.Web
Imports System.Web.Routing
Imports Microsoft.AspNet.FriendlyUrls

Namespace WebForms
	Public NotInheritable Class RouteConfig
		Private Sub New()
		End Sub
		Public Shared Sub RegisterRoutes(routes As RouteCollection)
			Dim settings As FriendlyUrlSettings = New FriendlyUrlSettings()
			settings.AutoRedirectMode = RedirectMode.Permanent
			routes.EnableFriendlyUrls(settings)
		End Sub
	End Class
End Namespace
