Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Routing
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports Microsoft.AspNet.FriendlyUrls.Resolvers

Namespace WebForms
	Public Partial Class ViewSwitcher
		Inherits System.Web.UI.UserControl
		Protected Property CurrentView() As String
			Get
				Return m_CurrentView
			End Get
			Private Set
				m_CurrentView = Value
			End Set
		End Property
		Private m_CurrentView As String

		Protected Property AlternateView() As String
			Get
				Return m_AlternateView
			End Get
			Private Set
				m_AlternateView = Value
			End Set
		End Property
		Private m_AlternateView As String

		Protected Property SwitchUrl() As String
			Get
				Return m_SwitchUrl
			End Get
			Private Set
				m_SwitchUrl = Value
			End Set
		End Property
		Private m_SwitchUrl As String

		Protected Sub Page_Load(sender As Object, e As EventArgs)
			' Determine current view
			Dim isMobile = WebFormsFriendlyUrlResolver.IsMobileView(New HttpContextWrapper(Context))
			CurrentView = If(isMobile, "Mobile", "Desktop")

			' Determine alternate view
			AlternateView = If(isMobile, "Desktop", "Mobile")

			' Create switch URL from the route, e.g. ~/__FriendlyUrls_SwitchView/Mobile?ReturnUrl=/Page
			Dim switchViewRouteName = "AspNet.FriendlyUrls.SwitchView"
			Dim switchViewRoute = RouteTable.Routes(switchViewRouteName)
			If switchViewRoute Is Nothing Then
				' Friendly URLs is not enabled or the name of the switch view route is out of sync
				Me.Visible = False
				Return
			End If
			Dim url = GetRouteUrl(switchViewRouteName, New With { _
				Key .view = AlternateView, _
				Key .__FriendlyUrls_SwitchViews = True _
			})
			url += "?ReturnUrl=" & HttpUtility.UrlEncode(Request.RawUrl)
			SwitchUrl = url
		End Sub
	End Class
End Namespace
