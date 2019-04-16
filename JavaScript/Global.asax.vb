Imports System.Web
Imports System.Web.Mvc
Imports System.Web.Routing
Imports ActiveQueryBuilder.Web.Server

Public Class MvcApplication
	Inherits HttpApplication
	Protected Sub Application_Start()
		AreaRegistration.RegisterAllAreas()
		FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
		RouteConfig.RegisterRoutes(RouteTable.Routes)

		' Instructing to initialize new instances of the QueryBuilder object according to 
		' directives in the special configuration section of 'Web.config' file.

		' Uncomment this line to work with the "Create Query Configuration from Web.Config" demo
		' QueryBuilderStore.UseWebConfig();
	End Sub
End Class
